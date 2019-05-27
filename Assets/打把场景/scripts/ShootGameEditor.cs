using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Battlehub.RTHandles.Demo;
using Battlehub.RTCommon;
using System.Linq;
using Battlehub.RTHandles;
using System.Text.RegularExpressions;

[DefaultExecutionOrder(-10)]
public class ShootGameEditor : SimpleEditor
{
    public static ShootGameEditor _Instance;

    private List<ShootingArea> m_Arealist = new List<ShootingArea>();

    public List<ShootingArea> Arealist
    {
        get
        {
            return m_Arealist;
        }
    }

    [SerializeField]
    private GameObject AreaCount = null;
    [SerializeField]
    private GameObject AreaItemCount = null;
    bool Lock = false;


    public bool IsCreated
    {
        get { return true; }
    }

    private GameObject EditorArea;

    public GameObject m_editorArea
    {
        get
        {
            return EditorArea;
        }
    }

    #region IRTEState implementation
    public event System.Action<object> Created;
    public event System.Action<object> Destroyed;
    private void Use()
    {
        Created(null);
        Destroyed(null);
    }
    #endregion

    protected override void Awake()
    {
        base.Awake();
        _Instance = this;
    }

    protected override void Start()
    {
        base.Start();

        EditorUI._Instance.areaListUI.Add.onClick.AddListener(delegate () { Add_Arealist(); });
        EditorUI._Instance.areaListUI.Subtract.onClick.AddListener(delegate () { Subtract_Arealist(Editor.Selection.gameObjects); });
        EditorUI._Instance.runtimeToolUI.Move_Button.onClick.AddListener(delegate () { SetRuntimeTool(RuntimeTool.Move); });
        EditorUI._Instance.runtimeToolUI.Rotate_Button.onClick.AddListener(delegate () { SetRuntimeTool(RuntimeTool.Rotate); });
        EditorUI._Instance.runtimeToolUI.Scale_Button.onClick.AddListener(delegate () { SetRuntimeTool(RuntimeTool.Scale); });
        EditorUI._Instance.runtimeToolUI.Lock_Button.onClick.AddListener(delegate () { SetRuntimeTool(RuntimeTool.Lock); });
        EditorUI._Instance.runtimeToolUI.View_Button.onClick.AddListener(delegate () { OnPlayClick(); });
        EditorUI._Instance.areaEditorUI.m_time.onEndEdit.AddListener(delegate (string value) { GetEditorArea().AreaTime = int.Parse(value); });
        EditorUI._Instance.areaEditorUI.m_shootNum.onEndEdit.AddListener(delegate (string value) { GetEditorArea().AreaShootNum = int.Parse(value); });
        EditorUI._Instance.itemListUI.Paper.onClick.AddListener(delegate () { ChoiceType(ItemType.shootingPaperTargets); });
        EditorUI._Instance.itemListUI.Steel.onClick.AddListener(delegate () { ChoiceType(ItemType.shootingSteelTarget); });
        EditorUI._Instance.itemListUI.Move.onClick.AddListener(delegate () { ChoiceType(ItemType.shootingMoveTarget); });
        EditorUI._Instance.itemListUI.Environment.onClick.AddListener(delegate () { ChoiceType(ItemType.EnvironmentTarget); });
        EditorUI._Instance.itemListUI.Ambient.onClick.AddListener(delegate () { ChoiceType(ItemType.ambientTarget); });

        EditorUI._Instance.itemEditorUI.CanThought.onValueChanged.AddListener(delegate (bool value)
        {
            ShootingItem item = getActiveItem(Editor.Selection.activeGameObject);
            item.CanThought = value;
            for (int i = 0; i < GetEditorArea().m_ShootingItem.Count; i++)
            {
                if (item.Prefab == GetEditorArea().m_ShootingItem[i].Prefab)
                {
                    GetEditorArea().m_ShootingItem[i] = item;
                    break;
                }
            }
        });
        EditorUI._Instance.itemEditorUI.ProhibitShooting.onValueChanged.AddListener(delegate (bool value)
        {
            ShootingItem item = getActiveItem(Editor.Selection.activeGameObject);
            item.ProhibitShooting = value;
            for (int i = 0; i < GetEditorArea().m_ShootingItem.Count; i++)
            {
                if (item.Prefab == GetEditorArea().m_ShootingItem[i].Prefab)
                {
                    GetEditorArea().m_ShootingItem[i] = item;
                    break;
                }
            }
        });
        EditorUI._Instance.itemEditorUI.InvalidItem.onValueChanged.AddListener(delegate (bool value)
        {
            ShootingItem item = getActiveItem(Editor.Selection.activeGameObject);
            item.InvalidItem = value;
            for (int i = 0; i < GetEditorArea().m_ShootingItem.Count; i++)
            {
                if (item.Prefab == GetEditorArea().m_ShootingItem[i].Prefab)
                {
                    GetEditorArea().m_ShootingItem[i] = item;
                    break;
                }
            }
        });
        //Editor.Selection.SelectionChanged += test;
        Editor.Selection.Selectioned += refreshUI;
        Editor.Mask = (1 << LayerMask.NameToLayer("Area"));
        Set_LockItemList(Lock, null);
        Editor.Tools.LockAxes = new LockObject
        {
            PositionY = true,
            ScaleY = true,
            RotationX = true,
            RotationZ = true,
            RotationFree = true,
            RotationScreen = true

        };
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            Xml_ShootingItem.DeleteXmlByPath();
            Xml_ShootingItem.CreateXml();
            int id = 0;
            foreach (ShootingArea sa in m_Arealist)
            {
                foreach (ShootingItem sItem in sa.m_ShootingItem)
                {
                    ++id;        
                    Xml_ShootingItem.AddXmlData(sItem, id);
                }
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
    //增加Area
    void Add_Arealist()
    {
        ShootingArea newArea = new ShootingArea();
        int num = 1;
        bool iscreat = false;

        while (!iscreat)
        {
            iscreat = true;
            if (m_Arealist.Count != 0)
                for (int i = 0; i < m_Arealist.Count; i++)
                {
                    if (m_Arealist[i].m_Number == num)
                    {
                        iscreat = false;
                        ++num;
                        break;
                    }
                }

        }

        newArea.Instantiate_obj(Resources.Load<GameObject>("ShootingArea")
                                , EditorUI._Instance.areaListUI.AreaPerfabUI
                                , AreaCount, EditorUI._Instance.areaListUI.AreaUICount
                                , delegate () { UIClickEvent(newArea); }
                                , num);
        newArea.ItemList = new ShootingArea.ShootingItemList();
        newArea.AddItem(Resources.Load<GameObject>("区域射击点"), getAreaItemListSprit("区域射击点"), AreaItemCount.transform);
        newArea.ItemList.AddItem(getShootingAreaItemList("钢靶(红)大"), getShootingAreaItemListSprit("钢靶(红)大"), ItemType.shootingSteelTarget);
        newArea.ItemList.AddItem(getShootingAreaItemList("钢靶(白)大"), getShootingAreaItemListSprit("钢靶(白)大"), ItemType.shootingSteelTarget);
        newArea.ItemList.AddItem(getShootingAreaItemList("钢靶(蓝)大"), getShootingAreaItemListSprit("钢靶(蓝)大"), ItemType.shootingSteelTarget);

        newArea.ItemList.AddItem(getShootingAreaItemList("铁丝网"), getShootingAreaItemListSprit("铁丝网"), ItemType.ambientTarget);
        newArea.ItemList.AddItem(getShootingAreaItemList("铁丝网(门)"), getShootingAreaItemListSprit("铁丝网(门)"), ItemType.ambientTarget);

        newArea.ItemList.AddItem(getShootingAreaItemList("方形钢靶(白)"), getShootingAreaItemListSprit("方形钢靶(白)"), ItemType.shootingSteelTarget);
        newArea.ItemList.AddItem(getShootingAreaItemList("方形钢靶(蓝)"), getShootingAreaItemListSprit("方形钢靶(蓝)"), ItemType.shootingSteelTarget);
        newArea.ItemList.AddItem(getShootingAreaItemList("方形钢靶(红)"), getShootingAreaItemListSprit("方形钢靶(红)"), ItemType.shootingSteelTarget);

        newArea.ItemList.AddItem(getShootingAreaItemList("圆形钢靶(白)"), getShootingAreaItemListSprit("圆形钢靶(白)"), ItemType.shootingSteelTarget);
        newArea.ItemList.AddItem(getShootingAreaItemList("圆形钢靶(蓝)"), getShootingAreaItemListSprit("圆形钢靶(蓝)"), ItemType.shootingSteelTarget);
        newArea.ItemList.AddItem(getShootingAreaItemList("圆形钢靶(红)"), getShootingAreaItemListSprit("圆形钢靶(红)"), ItemType.shootingSteelTarget);
        m_Arealist.Add(newArea);

        List<Object> selection;
        selection = new List<Object>();
        selection.Insert(0, newArea.Perfab);
        Editor.Undo.Select(selection.ToArray(), newArea.Perfab);

        Associated();
    }

    GameObject getShootingAreaItemList(string name)
    {
        return Resources.Load<GameObject>(name);
    }
    Sprite getShootingAreaItemListSprit(string name)
    {
        return Resources.Load<Sprite>("UI/Sprite/large/" + name);
    }
    Sprite getAreaItemListSprit(string name)
    {
        return Resources.Load<Sprite>("UI/Sprite/small/" + name);
    }

    //增加Area的Item
    public void Add_AreaItemList(GameObject prefabInstance, int number)
    {
        GetEditorArea().ItemList.m_ShootingItem.ForEach(item =>
        {
            if (item.Number == number)
            {
                ShootingItem newItem = new ShootingItem();
                newItem = item;
                newItem.MinImage = getAreaItemListSprit(newItem.MinImage.name);
                newItem.Prefab = prefabInstance;
                newItem.Prefab.transform.parent = AreaItemCount.transform.Find(GetEditorArea().Perfab.name);
                GetEditorArea().m_ShootingItem.Add(newItem);
                Debug.Log(item.Name);
            }
        });
    }
    //获取锁定Area的GameObject类型
    GameObject[] GetEditorAreaObj()
    {
        GameObject[] selection = new GameObject[1];
        selection[0] = EditorArea;
        return selection;
    }
    void test(Object[] unselected)
    {
        if (unselected != null)
            for (int i = 0; i < unselected.Length; ++i)
            {
                GameObject unselectedObj = unselected[i] as GameObject;
                Debug.Log(unselectedObj.name);
            }
    }
    //监听选中物体的的UI变化
    void refreshUI(Object[] selected)
    {
        bool Postion = true;
        bool Rotation = true;
        bool Scale = true;
        bool Locks = true;
        if (!Lock)
            CleanItemList();

        EditorUI._Instance.itemEditorUI.gameObject.SetActive(false);

        if ((selected != null && selected.Length != 0) || EditorArea != null)
        {
            GameObject[] m_selected = new GameObject[1];

            if (EditorArea != null)
                m_selected[0] = EditorArea;
            else
                m_selected[0] = selected[0] as GameObject;
            if (m_selected[0].layer == 10)
            {
                m_Arealist.ForEach(item =>
                {
                    if (item.Perfab == m_selected[0])
                    {
                        item.ItemList.Instantiate_obj(EditorUI._Instance.itemListUI.Prefab, EditorUI._Instance.itemListUI.ItemListUICount.gameObject, itemListType);
                        item.Instantiate_Item(EditorUI._Instance.areaItemListUI.AreaItemListPrefabUI, EditorUI._Instance.areaItemListUI.AreaItemListUICount, Editor);
                        int areaTime;
                        int areaShootNum;
                        item.getAreaMessage(out areaTime, out areaShootNum);
                        EditorUI._Instance.areaEditorUI.gameObject.SetActive(true);
                        if (Lock)
                            EditorUI._Instance.areaEditorUI.enable();
                        else
                            EditorUI._Instance.areaEditorUI.disable();
                        EditorUI._Instance.areaEditorUI.setvalue(areaTime, areaShootNum);
                    }
                });
            }
            else
                Locks = false;



            if (selected != null)
            {
                for (int i = 0; i < selected.Length; ++i)
                {
                    GameObject selectedObj = selected[i] as GameObject;
                    ExposeToEditor ObjEditor = selectedObj.GetComponent<ExposeToEditor>();
                    if (ObjEditor)
                    {
                        foreach (DisabledToolsType item in ObjEditor.DisabledToolsType)
                        {
                            if (item == DisabledToolsType.Position && Postion)
                                Postion = false;
                            if (item == DisabledToolsType.Roation && Rotation)
                                Rotation = false;
                            if (item == DisabledToolsType.Scale && Scale)
                                Scale = false;
                        }
                    }

                    if (selectedObj.layer == LayerMask.NameToLayer("Item"))
                    {
                        EditorUI._Instance.itemEditorUI.gameObject.SetActive(true);
                        ShootingItem item = getActiveItem(selectedObj);
                        EditorUI._Instance.itemEditorUI.ItemName.text = item.Name;
                        ItemType type = item.Type;
                        switch (type)
                        {
                            case ItemType.EnvironmentTarget:
                                EditorUI._Instance.itemEditorUI.CanThought.gameObject.SetActive(true);
                                EditorUI._Instance.itemEditorUI.CanThought.isOn = item.CanThought;
                                EditorUI._Instance.itemEditorUI.ProhibitShooting.gameObject.SetActive(false);
                                EditorUI._Instance.itemEditorUI.InvalidItem.gameObject.SetActive(false);
                                break;
                            case ItemType.shootingMoveTarget:
                                EditorUI._Instance.itemEditorUI.CanThought.gameObject.SetActive(true);
                                EditorUI._Instance.itemEditorUI.CanThought.isOn = item.CanThought;
                                EditorUI._Instance.itemEditorUI.ProhibitShooting.gameObject.SetActive(true);
                                EditorUI._Instance.itemEditorUI.ProhibitShooting.isOn = item.ProhibitShooting;
                                EditorUI._Instance.itemEditorUI.InvalidItem.gameObject.SetActive(true);
                                EditorUI._Instance.itemEditorUI.InvalidItem.isOn = item.InvalidItem;
                                break;
                            case ItemType.shootingPaperTargets:
                                EditorUI._Instance.itemEditorUI.CanThought.gameObject.SetActive(true);
                                EditorUI._Instance.itemEditorUI.CanThought.isOn = item.CanThought;
                                EditorUI._Instance.itemEditorUI.ProhibitShooting.gameObject.SetActive(true);
                                EditorUI._Instance.itemEditorUI.ProhibitShooting.isOn = item.ProhibitShooting;
                                EditorUI._Instance.itemEditorUI.InvalidItem.gameObject.SetActive(true);
                                EditorUI._Instance.itemEditorUI.InvalidItem.isOn = item.InvalidItem;
                                break;
                            case ItemType.shootingSteelTarget:
                                EditorUI._Instance.itemEditorUI.CanThought.gameObject.SetActive(true);
                                EditorUI._Instance.itemEditorUI.CanThought.isOn = item.CanThought;
                                EditorUI._Instance.itemEditorUI.ProhibitShooting.gameObject.SetActive(true);
                                EditorUI._Instance.itemEditorUI.ProhibitShooting.isOn = item.ProhibitShooting;
                                EditorUI._Instance.itemEditorUI.InvalidItem.gameObject.SetActive(true);
                                EditorUI._Instance.itemEditorUI.InvalidItem.isOn = item.InvalidItem;
                                break;
                            case ItemType.shootingPos:
                                EditorUI._Instance.itemEditorUI.CanThought.gameObject.SetActive(false);
                                EditorUI._Instance.itemEditorUI.ProhibitShooting.gameObject.SetActive(false);
                                EditorUI._Instance.itemEditorUI.InvalidItem.gameObject.SetActive(false);
                                break;

                        }
                    }
                }
            }

        }
        else
        {
            Postion = false;
            Rotation = false;
            Scale = false;
            Locks = false;
            EditorUI._Instance.areaEditorUI.gameObject.SetActive(false);

        }
        EditorUI._Instance.runtimeToolUI.Move_Button.interactable = Postion;
        EditorUI._Instance.runtimeToolUI.Rotate_Button.interactable = Rotation;
        EditorUI._Instance.runtimeToolUI.Scale_Button.interactable = Scale;
        EditorUI._Instance.runtimeToolUI.Lock_Button.interactable = Locks;

        Set_LockItemList(Lock, selected);
    }


    //清除itemList和AreaItemList
    void CleanItemList()
    {
        for (int i = 0; i < EditorUI._Instance.itemListUI.ItemListUICount.gameObject.transform.childCount; i++)
        {
            Destroy(EditorUI._Instance.itemListUI.ItemListUICount.gameObject.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < EditorUI._Instance.areaItemListUI.AreaItemListUICount.gameObject.transform.childCount; i++)
        {
            Destroy(EditorUI._Instance.areaItemListUI.AreaItemListUICount.gameObject.transform.GetChild(i).gameObject);
        }
    }
    //设置锁定状态下itemList是否可拖动
    void Set_LockItemList(bool value, Object[] selected)
    {
        EditorUI._Instance.itemListUI.LockUI.gameObject.SetActive(!value);

        EditorUI._Instance.areaListUI.LockUI.gameObject.SetActive(value);

        EditorUI._Instance.areaItemListUI.LockUI.gameObject.SetActive(!value);

        m_Arealist.ForEach(item =>
        {
            item.Perfab.GetComponent<MeshRenderer>().enabled = false;
        });

        for (int i = 0; i < EditorUI._Instance.itemListUI.ItemListUICount.gameObject.transform.childCount; i++)
        {
            EditorUI._Instance.itemListUI.ItemListUICount.gameObject.transform.GetChild(i).GetComponent<PrefabSpawnPoint>().CanDrag = value;

        }
        for (int i = 0; i < EditorUI._Instance.areaListUI.AreaUICount.gameObject.transform.childCount; i++)
        {
            EditorUI._Instance.areaListUI.AreaUICount.gameObject.transform.GetChild(i).GetComponent<Button>().interactable = !value;
            EditorUI._Instance.areaListUI.AreaUICount.gameObject.transform.GetChild(i).transform.Find("Sprite").GetComponent<Image>().sprite =
                EditorUI._Instance.areaListUI.ordinary;
        }
        for (int i = 0; i < EditorUI._Instance.areaItemListUI.AreaItemListUICount.gameObject.transform.childCount; i++)
        {
            EditorUI._Instance.areaItemListUI.AreaItemListUICount.gameObject.transform.GetChild(i).GetComponent<Button>().interactable = value;
            EditorUI._Instance.areaItemListUI.AreaItemListUICount.gameObject.transform.GetChild(i).transform.Find("Sprite").GetComponent<Image>().sprite =
                EditorUI._Instance.areaItemListUI.ordinary;
        }
        EditorUI._Instance.areaListUI.Add.interactable = !value;
        EditorUI._Instance.areaListUI.Subtract.interactable = !value;

        if (EditorArea)
        {
            GetEditorArea().PerfabUI.transform.Find("Sprite").GetComponent<Image>().sprite = EditorUI._Instance.areaListUI.active;
            GetEditorArea().Perfab.GetComponent<MeshRenderer>().enabled = true;
        }


        if (selected != null && selected.Length > 0)
        {
            if ((selected[0] as GameObject).layer == LayerMask.NameToLayer("Area"))
            {
                GetActiveArea(selected[0] as GameObject).PerfabUI.transform.Find("Sprite").GetComponent<Image>().sprite = EditorUI._Instance.areaListUI.active;
                GetActiveArea(selected[0] as GameObject).Perfab.GetComponent<MeshRenderer>().enabled = true;
            }
            if ((selected[0] as GameObject).layer == LayerMask.NameToLayer("Item") || (selected[0] as GameObject).layer == LayerMask.NameToLayer("ShootPos"))
            {
                getActiveItem(selected[0] as GameObject).PrefabUI.transform.Find("Sprite").GetComponent<Image>().sprite = EditorUI._Instance.areaItemListUI.active;
            }
        }
    }
    //ShootingArea列表的点击事件
    void UIClickEvent(ShootingArea value)
    {
        List<Object> selection;

        selection = new List<Object>();

        selection.Insert(0, value.Perfab);

        Editor.Undo.Select(selection.ToArray(), value.Perfab);
    }

    ItemType itemListType = ItemType.shootingPaperTargets;

    ItemType m_itemListType
    {
        get { return itemListType; }
        set
        {
            itemListType = value;
            refreshUI(Editor.Selection.objects);
        }
    }
    void ChoiceType(ItemType type)
    {
        m_itemListType = type;
    }


    //设置Tools种类
    void SetRuntimeTool(RuntimeTool runtimeTool)
    {
        switch (runtimeTool)
        {
            case RuntimeTool.Move:
                Editor.Tools.Current = RuntimeTool.Move;
                break;
            case RuntimeTool.Rotate:
                Editor.Tools.Current = RuntimeTool.Rotate;
                break;
            case RuntimeTool.Scale:
                Editor.Tools.Current = RuntimeTool.Scale;
                break;
            case RuntimeTool.Lock:
                if (!Lock)
                    LockAcitiveArea();
                else
                    UnLockAcitiveArea();
                break;
        }
    }

    //list换位排序
    public void ShootAreaSorting()
    {
        List<ShootingArea> newlist = new List<ShootingArea>();

        foreach (Transform i in EditorUI._Instance.areaListUI.AreaUICount.transform)
        {
            m_Arealist.ForEach(item =>
            {
                if (i.gameObject == item.PerfabUI)
                    newlist.Add(item);
            });
        }

        m_Arealist = newlist;

        m_Arealist.ForEach(item =>
        {
            Debug.Log(item.m_Number);
        });
    }

    //锁定Area
    void LockAcitiveArea()
    {
        GameObject[] selected = Editor.Selection.gameObjects;
        Lock = true;
        if (selected != null)
        {
            for (int i = 0; i < selected.Length; ++i)
            {
                GameObject selectedObj = selected[i];
                EditorArea = selectedObj;
                //Debug.Log(selectedObj.name);
                //ExposeToEditor exposeToEditor = EditorArea.GetComponent<ExposeToEditor>();
                //SelectionGizmo selectionGizmo = EditorArea.AddComponent<SelectionGizmo>();

                //if (exposeToEditor.Selected != null)
                //{
                //    exposeToEditor.Selected.Invoke(exposeToEditor);
                //}
            }
        }
        //Editor.Selection.activeObject = null;

        Editor.Undo.Select(null, null);

        Editor.Tools.Current = RuntimeTool.Move;

        Editor.Mask = Editor.Mask = (1 << LayerMask.NameToLayer("ShootPos") | 1 << LayerMask.NameToLayer("Item"));
    }

    //解锁Area
    void UnLockAcitiveArea()
    {
        Lock = false;
        List<Object> selection;

        selection = new List<Object>();

        selection.Insert(0, EditorArea);

        Editor.Undo.Select(selection.ToArray(), EditorArea);

        Editor.Mask = Editor.Mask = (1 << LayerMask.NameToLayer("Area"));

        //Set_LockItemList(false);

        Editor.Tools.Current = RuntimeTool.Move;
        EditorArea = null;
    }

    //ShootAreaList选定删除
    void Subtract_Arealist(GameObject[] area)
    {
        if (area.Length == 0)
            return;

        List<int> m_Area = getShootingArea(area);

        foreach (int i in m_Area)
        {
            m_Arealist[i].Destroy_obj(AreaItemCount);
            m_Arealist.RemoveAt(i);
        }
    }
    //获取参数中物体在ShootAreaList中的标记
    List<int> getShootingArea(GameObject[] values)
    {
        List<int> m_Area = new List<int>();
        for (int i = 0; i < m_Arealist.Count; i++)
        {
            foreach (GameObject j in values)
            {
                if (m_Arealist[i].Perfab.name == j.name)
                {
                    m_Area.Add(i);
                }
            }
        }
        return m_Area;
    }

    int getShootingArea(GameObject values)
    {
        int m_Area = new int();
        for (int i = 0; i < m_Arealist.Count; i++)
        {

            if (m_Arealist[i].Perfab.name == values.name)
            {
                m_Area = i;
                break;
            }

        }
        return m_Area;
    }
    //获取锁定Area的ShootingArea属性结构
    ShootingArea GetEditorArea()
    {
        return m_Arealist[getShootingArea(GetEditorAreaObj())[0]];
    }

    ShootingArea GetActiveArea(GameObject value)
    {
        return m_Arealist[getShootingArea(value)];
    }


    //获取选中item
    ShootingItem getActiveItem(GameObject value)
    {
        ShootingItem newItem = new ShootingItem();
        GetEditorArea().m_ShootingItem.ForEach(item =>
        {
            if (item.Prefab == value)
                newItem = item;
        });
        Debug.Log(newItem.PrefabUI.name);
        return newItem;
    }

    [SerializeField]
    bool isPlaying = false;
    private void OnPlayClick()
    {
        isPlaying = !isPlaying;
        ShootGame._Instance.isEditor = !isPlaying;
        if (isPlaying)
        {
            //Editor.Undo.Purge();
            CoPlay();
        }
        else
            OnStopClick();

        ShootGame._Instance.Start();
    }

    private void CoPlay()
    {
        if (Lock)
            UnLockAcitiveArea();
        Editor.Undo.Select(null, null);
        Editor.Selection.Enabled = false;
        EditorUI._Instance.playingModle();
        Associated();
    }
    private void OnStopClick()
    {
        Editor.Selection.Enabled = true;
        Editor.Undo.Select(null, null);
        EditorUI._Instance.editorModle();
    }

    //射击点与靶子关联
    void Associated()
    {
        m_Arealist.ForEach(item =>
        {
            //InteractiveSwitch Point = new InteractiveSwitch();
            List<TargetHealth> targets = new List<TargetHealth>();
            item.m_ShootingItem.ForEach(obj =>
            {
                if (obj.Prefab.GetComponent<InteractiveSwitch>())
                {
                    //Point = obj.Prefab.GetComponent<InteractiveSwitch>();
                    item.m_ShootPos = obj;
                }
                if (obj.Prefab.GetComponent<TargetHealth>())
                    targets.Add(obj.Prefab.GetComponent<TargetHealth>());
            });
            item.m_ShootPos.Prefab.GetComponent<InteractiveSwitch>().targets = targets;
        });
        if (m_Arealist.Count == 1)
        {
            m_Arealist.First<ShootingArea>().m_ShootPos.Prefab.GetComponent<InteractiveSwitch>().startVisible = true;
            m_Arealist.First<ShootingArea>().m_ShootPos.Prefab.GetComponent<InteractiveSwitch>().levelEnd = true;
        }
        if (m_Arealist.Count > 1)
        {
            m_Arealist.First<ShootingArea>().m_ShootPos.Prefab.GetComponent<InteractiveSwitch>().startVisible = true;
            m_Arealist.First<ShootingArea>().m_ShootPos.Prefab.GetComponent<InteractiveSwitch>().levelEnd = false;
            m_Arealist.Last<ShootingArea>().m_ShootPos.Prefab.GetComponent<InteractiveSwitch>().startVisible = false;
            m_Arealist.Last<ShootingArea>().m_ShootPos.Prefab.GetComponent<InteractiveSwitch>().levelEnd = true;
        }

        for (int i = 0; i < m_Arealist.Count; i++)
        {
            if (i + 1 < m_Arealist.Count)
                m_Arealist[i].m_ShootPos.Prefab.GetComponent<InteractiveSwitch>().nextStage =
                    m_Arealist[i + 1].m_ShootPos.Prefab.GetComponent<InteractiveSwitch>();
        }
        m_Arealist.Last<ShootingArea>().m_ShootPos.Prefab.GetComponent<InteractiveSwitch>().nextStage = null;
    }

}
