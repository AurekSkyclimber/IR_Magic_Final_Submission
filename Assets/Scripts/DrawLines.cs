using System.Collections;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class DrawLines : MonoBehaviour {
    // General Vars
    public bool Mock; // Mouse control active for testing?

    Thread _wandPosThread;
    UdpClient _wandPosClient;
    private const int kWandPosPort = 5065;

    private Vector2 _pos = Vector2.one;
    private bool _newPos = false;
    private bool _spellActive = false;

    private enum LineState { Pointing = 0, Drawing }
    private LineState _lineState = LineState.Pointing;

    // CNN Vars
    private LineRenderer _lineRenderer;
    private Transform _pointer;

    private Camera _cam;
    private Vector3 _screenStart;
    private Vector3 _screenSize;

    private int _imgSize = 28;
    private bool _savePhoto = false;

    // Magic Vars
    private Transform _magicPos;
    private LineRenderer _magicLine;
    private SpriteRenderer _magicPoint;

    private TrailRenderer _rainbowLine;
    private ParticleSystem _flowerParticles;
    private GameObject _bat;

    private TcpClientManager _tcpClient;
    private enum SpellShapes { Left = 0, M, O, Right, S }
    private SpellShapes _shape;

    private MeshRenderer _leftPage;
    private MeshRenderer _rightPage;
    private SkinnedMeshRenderer _middlePage;
    private Animator _pageTurn;
    private Material[] _pageMats = new Material[3];
    private int _currentPage = 0;

    // UNITY MAIN LOOP

    private void Awake() {
        _lineRenderer = GetComponent<LineRenderer>();
        _pointer = GameObject.Find("Pointer").transform;

        _cam = GameObject.Find("Drawing Camera").GetComponent<Camera>();
        _screenStart = new Vector3(-_cam.orthographicSize * _cam.aspect, -_cam.orthographicSize, 0);
        _screenSize = new Vector3(_screenStart.x * -2, _screenStart.y * -2, 0);

        _magicPos = GameObject.Find("MagicLine").transform;
        _magicLine = _magicPos.GetComponent<LineRenderer>();
        _magicLine.enabled = true;
        _magicPoint = _magicPos.GetComponent<SpriteRenderer>();
        _magicPoint.enabled = true;

        _rainbowLine = _magicPos.GetComponent<TrailRenderer>();
        _rainbowLine.enabled = false;
        _flowerParticles = _magicPos.GetComponent<ParticleSystem>();
        _bat = GameObject.Find("Bat");
        _bat.SetActive(false);

        _leftPage = _pageTurn.transform.Find("Inner3b_low").GetComponent<MeshRenderer>();
        _rightPage = _pageTurn.transform.Find("Inner3a_low").GetComponent<MeshRenderer>();
        _middlePage = _pageTurn.transform.Find("Inner3a_low_001").GetComponent<SkinnedMeshRenderer>();
        _pageTurn = GameObject.Find("PoseableBook").GetComponent<Animator>();

        for (int i = 0; i < _pageMats.Length; i++) {
            _pageMats[i] = Resources.Load<Material>("Materials/Page " + i);
        }
    }

    private void Start() {
        if (!Mock) {
            _wandPosThread = new Thread(new ThreadStart(ReceiveWandPos));
            _wandPosThread.IsBackground = true;
            _wandPosThread.Start();
        }

        _tcpClient = GetComponent<TcpClientManager>();
        _tcpClient.setupSocket();
    }

    private void Update () {
        if (Mock) {
            _pos = new Vector2(1 - (Input.mousePosition.x / (float)Screen.width), 1 - (Input.mousePosition.y / (float)Screen.height));
            _newPos = true;
        }

        if (!_spellActive && Input.GetKeyDown(KeyCode.Space)) {
            if (_lineState.Equals(LineState.Pointing)) {
                _pointer.gameObject.SetActive(false);
                _magicPoint.enabled = false;
                _magicLine.enabled = true;
                ResetLines();
                _lineState = LineState.Drawing;
            } else {
                _spellActive = true;
                _savePhoto = true;
            }
        }

        if (_newPos) {
            _newPos = false;
            _magicPos.position = ConvertPctToMagicSpace(_pos);
            if (!_spellActive) {
                if (_lineState.Equals(LineState.Pointing)) {
                    _pointer.position = ConvertPctToCNNSpace(_pos);
                } else {
                    _lineRenderer.positionCount++;
                    _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, ConvertPctToCNNSpace(_pos));
                    _magicLine.positionCount++;
                    _magicLine.SetPosition(_lineRenderer.positionCount - 1, ConvertPctToMagicSpace(_pos));
                }
            }
        }

        if (_tcpClient.newData) {
            Debug.Log("Shape is: " + _tcpClient.recievedData);
            _shape = (SpellShapes) int.Parse(_tcpClient.recievedData);
            _tcpClient.newData = false;
            StartCoroutine(CastSpell());
        }
    }

    // https://answers.unity.com/questions/22954/how-to-save-a-picture-take-screenshot-from-a-camer.html
    private void LateUpdate() {
        if (_savePhoto) {
            RenderTexture rt = new RenderTexture(_imgSize, _imgSize, 24);
            _cam.targetTexture = rt;
            Texture2D screenShot = new Texture2D(_imgSize, _imgSize, TextureFormat.RGB24, false);
            _cam.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, _imgSize, _imgSize), 0, 0);
            _cam.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = ScreenShotName();
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to: {0}", filename));
            _savePhoto = false;
            ResetLines();

            if (_tcpClient.connectionActive) {
                _tcpClient.writeSocket("Analyze");
            }
        }
    }

    private void OnDestroy() {
        if (!Mock) {
            if (_wandPosThread != null) {
                _wandPosThread.Abort();
                Debug.Log(_wandPosThread.IsAlive); //must be false
                _wandPosClient.Close();
            }
        }
    }

    // GENERAL

    private void ReceiveWandPos() {
        _wandPosClient = new UdpClient(kWandPosPort);
        while (true) {
            try {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), kWandPosPort);
                byte[] data = _wandPosClient.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);
                Debug.Log(text);
                string[] posString = text.Split(new char[] { ',' });
                _pos.x = float.Parse(posString[0]);
                _pos.y = float.Parse(posString[1]);
                _newPos = true;
                break;
            } catch (Exception e) {
                Debug.Log(e.ToString());
            }
        }
    }

    private void ResetLines() {
        _lineRenderer.positionCount = 0;
        _magicLine.positionCount = 0;
    }

    private string TrainingScreenShotName() {
        return string.Format("{0}/../screenshots/screen_{1}.png",
                             Application.dataPath,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    private string ScreenShotName() {
        return string.Format("{0}/../shape.png", Application.dataPath);
    }

    // CNN

    private Vector3 ConvertPctToCNNSpace(Vector2 wandPos) {
        return _screenStart + new Vector3((1 - wandPos.x) * _screenSize.x, (1 - wandPos.y) * _screenSize.y, 0f);
    }

    // MAGIC

    private Vector3 ConvertPctToMagicSpace(Vector2 wandPos) {
        return new Vector3((wandPos.x * _screenSize.x / 2f) + (_screenStart.x / 2f), (1 - wandPos.y) * _screenSize.y / 2f, -20f);
    }

    private IEnumerator CastSpell() {
        switch (_shape) {
            case SpellShapes.Left:
                int oldPage = _currentPage++;
                if (_currentPage > 2) { _currentPage = 0; }
                _middlePage.materials = new Material[] { _pageMats[oldPage], _pageMats[_currentPage] };
                _pageTurn.SetTrigger("RightToLeft");
                yield return new WaitForSeconds(0.1f);
                _rightPage.material = _pageMats[_currentPage];
                yield return new WaitForSeconds(1.8f);
                _leftPage.material = _pageMats[_currentPage];
                break;
            case SpellShapes.Right:
                oldPage = _currentPage--;
                if (_currentPage < 0) { _currentPage = 2; }
                _middlePage.materials = new Material[] { _pageMats[_currentPage], _pageMats[oldPage] };
                _pageTurn.SetTrigger("LeftToRight");
                yield return new WaitForSeconds(0.1f);
                _leftPage.material = _pageMats[_currentPage];
                yield return new WaitForSeconds(1.8f);
                _rightPage.material = _pageMats[_currentPage];
                break;
            case SpellShapes.M:
                _bat.SetActive(true);
                yield return new WaitForSeconds(2.26666666666666666666666666666667f);
                _bat.SetActive(false);
                break;
            case SpellShapes.O:
                _magicPoint.enabled = true;
                _flowerParticles.Play();
                yield return new WaitForSeconds(3);
                _flowerParticles.Stop();
                break;
            case SpellShapes.S:
                _rainbowLine.enabled = true;
                yield return new WaitForSeconds(3);
                _rainbowLine.enabled = false;
                break;
        }
        _magicPoint.enabled = true;
        _pointer.gameObject.SetActive(true);
        _lineState = LineState.Pointing;
        _spellActive = false;
    }
}
