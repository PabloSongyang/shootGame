// ==================================================================
// 作    者：Pablo.风暴洋-宋杨
// 説明する：Xml操作类
// 作成時間：2019-05-24
// 類を作る：Xml_ShootingItem.cs
// 版    本：v 1.0
// 会    社：大连仟源科技
// QQと微信：731483140
// ==================================================================

using System.Xml;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

public class Xml_ShootingItem : MonoBehaviour
{
    /// <summary>
    /// 文件名
    /// </summary>
    private static string ShootingItem = "ShootingItem";
    /// <summary>
    /// 文件路径
    /// </summary>
    private static string path = Application.dataPath + "/Xml/" + ShootingItem + ".xml";


    /// <summary>
    /// 创建Xml
    /// </summary>
    public static void CreateXml()
    {
        if (!File.Exists(path))
        {
            XmlDocument xml = new XmlDocument();
            //创建最上一层的节点
            XmlElement root = xml.CreateElement("ShootingItem");

            xml.AppendChild(root);
            xml.Save(path);
        }
    }

    /// <summary>
    /// 增加Xml数据
    /// </summary>
    /// <param name="sItem"></param>
    /// <param name="id"></param>
    public static void AddXmlData(ShootingItem sItem, int id)
    {
        if (File.Exists(path))
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(path);
            XmlNode root = xml.SelectSingleNode("ShootingItem");

            XmlElement element = xml.CreateElement("data");
            //设置节点的属性
            element.SetAttribute("id", id.ToString());

            SetElement(xml, element, sItem);
            root.AppendChild(element);

            xml.AppendChild(root);
            //最后保存文件
            xml.Save(path);
        }
    }

    /// <summary>
    /// 设置Xml元素
    /// </summary>
    /// <param name="xml"></param>
    /// <param name="element"></param>
    /// <param name="sItem"></param>
    private static void SetElement(XmlDocument xml, XmlElement element, ShootingItem sItem)
    {

        XmlElement xelItemType = xml.CreateElement("ItemType");

        //设置节点内面的内容
        xelItemType.InnerText = sItem.Type.ToString();

        XmlElement xelGeneral = xml.CreateElement("General");
        XmlElement xelposition = xml.CreateElement("position");
        xelposition.InnerText = sItem.m_General.position.ToString();
        XmlElement xelrotation = xml.CreateElement("rotation");
        xelrotation.InnerText = sItem.m_General.rotation.ToString();
        xelGeneral.AppendChild(xelposition);
        xelGeneral.AppendChild(xelrotation);

        XmlElement xelAction = xml.CreateElement("action");
        //设置节点内面的内容
        xelAction.InnerText = sItem.Event.ToString();

        XmlElement xelImage = xml.CreateElement("m_minImage");
        xelImage.InnerText = sItem.MinImage.name.ToString();

        XmlElement xelCanThought = xml.CreateElement("CanThought");
        xelCanThought.InnerText = sItem.CanThought.ToString();

        XmlElement xelProhibitShooting = xml.CreateElement("ProhibitShooting");
        xelProhibitShooting.InnerText = sItem.ProhibitShooting.ToString();

        XmlElement xelInvalidItem = xml.CreateElement("InvalidItem");
        xelInvalidItem.InnerText = sItem.InvalidItem.ToString();


        //把节点一层一层的添加至xml中，注意他们之间的先后顺序，这是生成XML文件的顺序
        element.AppendChild(xelItemType);
        element.AppendChild(xelAction);
        element.AppendChild(xelImage);

        element.AppendChild(xelCanThought);
        element.AppendChild(xelProhibitShooting);
        element.AppendChild(xelInvalidItem);
    }


    /// <summary>
    /// 删除单个条目数据
    /// </summary>
    public static void DeleteSingleXmlData(string index)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(path);
        XmlElement xe = doc.DocumentElement;
        string strPath = string.Format("/ShootingItem/data[@id=\"{0}\"]", index); //Xpath表达式
        XmlElement selectXe = (XmlElement)xe.SelectSingleNode(strPath);
        selectXe.ParentNode.RemoveChild(selectXe);
        doc.Save(path);
    }

    /// <summary>
    /// 获取所有的Xml数据
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<XElement> getAllXmlData()
    {
        XElement xe = XElement.Load(path);
        IEnumerable<XElement> elements = from ele in xe.Elements("data") select ele;
        return elements;
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    public static void DeleteXmlByPath()
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
