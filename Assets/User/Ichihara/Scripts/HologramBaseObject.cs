using UnityEngine;
using UniRx;

[RequireComponent(typeof(DefaultObserverEventHandler))]
public class HologramBaseObject : MonoBehaviour
{
    public bool isViewing { get; private set; }

    private DefaultObserverEventHandler handler = null;

    [SerializeField] private GameObject positionOffsetObject = null;
    [SerializeField] private GameObject pastObject = null;

    [SerializeField] private GameObject futureObject = null;

    [SerializeField] private bool isAlwaysShow = false;
    [SerializeField] private GameObject AlwaysShowObject;
    [SerializeField] private float AlwaysShowObjectForwardOffset = 0.6f;
    [SerializeField] private float AlwaysShowObjectDownOffset = 0.6f;
    private Transform rootTransform;

    /// <summary>
    /// 画像認識用の画像を一度でも読み込んでいるかどうか
    /// </summary>
    private bool viewedImage = false;

    private bool isFarstLook = false;

    void Start()
    {
        //初期化
        ObjectSetActive(futureObject, false);
        ObjectSetActive(pastObject, false);
        if (isAlwaysShow)
        {
            AlwaysShowObject.SetActive(false);
        }

        //移動と回転のルートオブジェクトを作成
        rootTransform = new GameObject(gameObject.name + "_RootObject").GetComponent<Transform>();
        //オフセット以下のオブジェクトをすべてルートの子要素に入れる
        var position = positionOffsetObject.transform.localPosition;
        var rotation = positionOffsetObject.transform.localRotation;
        positionOffsetObject.transform.parent = rootTransform;
        positionOffsetObject.transform.localPosition = position;
        positionOffsetObject.transform.localRotation = rotation;
        //イベントの登録
        handler = GetComponent<DefaultObserverEventHandler>();
        handler.OnTargetFound.AddListener(Viewing);
        handler.OnTargetLost.AddListener(Invisible);

        //時間軸の変更を購読
        TimeAxisManager.Instance.TimeAxisObserver.Subscribe(_ => { ShowObject(); }).AddTo(this);
    }


    private void Update()
    {
        //画像認識中のみ座標を更新
        if (isViewing)
        {
            rootTransform.position = transform.position;
            rootTransform.rotation = transform.rotation;
        }
    }

    public void Viewing()
    {
        viewedImage = true;
        isViewing = true;
        if (isAlwaysShow && !isFarstLook)
        {
            isFarstLook = true;
            AlwaysShowObject.SetActive(true);
            AlwaysShowObject.transform.parent = null;

            //暫定的にカメラの回転を制限したオブジェクトを生成し、前方向のベクトルを取得する
            //TODO:計算のみで実装
            var cameraTransform = Camera.main.transform;

            //上下の回転を制限
            var q = cameraTransform.rotation.eulerAngles;
            q.x = 0;
            q.z = 0;

            var tempGameObject = new GameObject("tempGameObject")
            {
                transform =
                {
                    rotation = Quaternion.Euler(q)
                }
            };

            var p = cameraTransform.position +
                    //前方向に座標を移動
                    tempGameObject.transform.forward * AlwaysShowObjectForwardOffset +
                    //カメラのY座標から一定値下にずらす
                    -Vector3.up * AlwaysShowObjectDownOffset;

            //座標を代入
            AlwaysShowObject.transform.position = p;


            ShowObject();
        }
    }

    public void Invisible()
    {
        isViewing = false;
        //初めてオブジェクトを見て安定してから親との連結を解除する
        /*if (!isFarstLook && isAlwaysShow)
        {
            AlwaysShowObject.transform.parent = null;
        }*/
    }

    private void ShowObject()
    {
        //画像を一度も読み込んでいない場合return
        if (!viewedImage)
        {
            return;
        }

        if (TimeAxisManager.Instance.Axis == TimeAxisManager.axis.future)
        {
            ObjectSetActive(futureObject, true);
            ObjectSetActive(pastObject, false);
        }
        else
        {
            ObjectSetActive(futureObject, false);
            ObjectSetActive(pastObject, true);
        }
    }

    void ObjectSetActive(GameObject gameObject, bool isActive)
    {
        if (gameObject)
        {
            gameObject.SetActive(isActive);
        }
    }
}