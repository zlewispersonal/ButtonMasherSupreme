using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class ComboTool : EditorWindow
{
    private const int NODE_WIDTH = 200;
    private const int NODE_HEIGHT = 400;

    private List<ComboNode> nodes;
    private List<Connection> connections;

    private Vector2 offset;
    private Vector2 drag;
    private GUIStyle node_style;
    private GUIStyle in_style;
    private GUIStyle out_style;

    private ConnectionPoint select_in_point;
    private ConnectionPoint select_out_point;

    [MenuItem("Window/Combo Tool")]
    public static void ShowWindow()
    {
        GetWindow<ComboTool>("Combo Tool");
    }

    private void OnEnable()
    {
        node_style = new GUIStyle();
        node_style.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node2.png") as Texture2D;
        node_style.border = new RectOffset(12, 12, 12, 12);

        in_style = new GUIStyle();
        in_style.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
        in_style.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        in_style.border = new RectOffset(4, 4, 12, 12);

        out_style = new GUIStyle();
        out_style.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
        out_style.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
        out_style.border = new RectOffset(4, 4, 12, 12);
    }

    void OnGUI()
    {
        // Window Code
        DrawGrid(20, 0.4f, Color.black);
        DrawNodes();
        DrawConnections();
        DrawConnectionLine(Event.current);

        ProcessNodeEvents(Event.current);
        ProcessEvents(Event.current);

        if (GUI.changed) Repaint();
    }

    void ProcessNodeEvents(Event e)
    {
        if (nodes != null)
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                bool guiChanged = nodes[i].ProcessEvents(e);

                if (guiChanged)
                {
                    GUI.changed = true;
                }
            }
        }
    }

    void ProcessEvents(Event e)
    {
        drag = Vector2.zero;

        //Switch case for different events, like right click or dragging
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 1)
                    ProcessContextMenu(e.mousePosition);
                if (e.button == 0 && (select_in_point != null || select_out_point != null))
                    ClearConnectionSelection();

                break;
            case EventType.MouseDrag:
                if (e.button == 0)
                    OnDrag(e.delta);
                break;
        }
    }

    //Right click options menu
    void ProcessContextMenu(Vector2 mouse_postion)
    {
        List<AttackObject> obj_names = new List<AttackObject>();
        DirectoryInfo info = new DirectoryInfo("Assets/Attack Objects/");
        FileInfo[] files = info.GetFiles();

        //Loop through and get a list of start combos
        foreach (FileInfo f in files)
        {
            if (!f.Name.Contains("meta"))
            {
                AttackObject a = (AttackObject)AssetDatabase.LoadAssetAtPath("Assets/Attack Objects/" + f.Name, typeof(AttackObject));
                if (a.parent == null)
                    obj_names.Add(a);
            }
        }

        GenericMenu genericMenu = new GenericMenu();
        if (nodes != null)
            genericMenu.AddItem(new GUIContent("Add Node"), false, () => CreateComboNode(mouse_postion));
        //Loop through and add options for each name
        foreach (AttackObject ao in obj_names)
            genericMenu.AddItem(new GUIContent("Open Combo/" + ao.name), false, () => OpenCombo(mouse_postion, ao));
        genericMenu.AddItem(new GUIContent("Make New Combo"), false, () => CreateNewCombo(mouse_postion));
        genericMenu.ShowAsContext();
    }


    //RIGHT CLICK, LEFT CLICK HELPER FUNCTIONS FOR PROCESS EVENT

    //Create the new atack object
    void CreateNewCombo(Vector2 mouse_position)
    {
        nodes = new List<ComboNode>();
        connections = new List<Connection>();

        AttackObject asset = CreateInstance<AttackObject>();
        AssetDatabase.CreateAsset(asset, "Assets/Attack Objects/New Attack Object.asset");
        AssetDatabase.SaveAssets();

        nodes.Add(new ComboNode(mouse_position, NODE_WIDTH, NODE_HEIGHT, node_style, in_style, out_style, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, asset));
    }

    //Create the new atack object
    void CreateComboNode(Vector2 mouse_position)
    {
        AttackObject asset = CreateInstance<AttackObject>();
        AssetDatabase.CreateAsset(asset, "Assets/Attack Objects/New Attack Object.asset");
        AssetDatabase.SaveAssets();

        nodes.Add(new ComboNode(mouse_position, NODE_WIDTH, NODE_HEIGHT, node_style, in_style, out_style, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, asset));
    }

    //Select option to open combo
    void OpenCombo(Vector2 mouse_postion, AttackObject ao)
    {
        nodes = new List<ComboNode>();
        connections = new List<Connection>();

        nodes.Add(new ComboNode(mouse_postion, NODE_WIDTH, NODE_HEIGHT, node_style, in_style, out_style, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, ao));

        RecursiveAddNode(nodes[0], mouse_postion);
    }

    void RecursiveAddNode(ComboNode ao, Vector2 last_position)
    {
        if (ao.attack_object.valid_moves == null)
            return;

        int counter = ao.attack_object.valid_moves.Count - 1;
        foreach(AttackObject c in ao.attack_object.valid_moves)
        {
            Vector2 new_postion = new Vector2(last_position.x + NODE_WIDTH + 100, last_position.y + (NODE_HEIGHT * counter));
            nodes.Add(new ComboNode(new_postion, NODE_WIDTH, NODE_HEIGHT, node_style, in_style, out_style, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, c));

            //Add connections
            select_out_point = ao.out_point;
            select_in_point = nodes[nodes.Count - 1].in_point;
            CreateConnection(false);
            ClearConnectionSelection();

            //Recurse
            RecursiveAddNode(nodes[nodes.Count - 1], new_postion);
            counter--;
        }
    }

    //Whenever the window is dragged, move everything
    void OnDrag(Vector2 delta)
    {
        drag = delta;

        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Drag(delta);
            }
        }

        GUI.changed = true;
    }

    void OnClickRemoveNode(ComboNode n)
    {
        //Remove it from parents valid moves
        if (n.attack_object.parent != null)
        {
            n.attack_object.parent.valid_moves.Remove(n.attack_object);
        }

        //Remove the parent from the children
        if(n.attack_object.valid_moves != null)
            foreach (AttackObject a in n.attack_object.valid_moves)
                a.parent = null;

        AssetDatabase.DeleteAsset("Assets/Attack Objects/" + n.attack_object.name + ".asset");
        AssetDatabase.SaveAssets();

        //Visual indications
        if (connections != null)
        {
            List<Connection> connectionsToRemove = new List<Connection>();

            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[i].in_point == n.in_point || connections[i].out_point == n.out_point)
                    connectionsToRemove.Add(connections[i]);
            }

            for (int i = 0; i < connectionsToRemove.Count; i++)
                connections.Remove(connectionsToRemove[i]);

            connectionsToRemove = null;
        }

        nodes.Remove(n);
    }


    //CONNECTION HELPER FUNCTIONS

    void OnClickInPoint(ConnectionPoint in_point)
    {
        select_in_point = in_point;

        if(select_out_point != null)
        {
            if (select_out_point.node != select_in_point.node)
                CreateConnection(true);

            ClearConnectionSelection();
        }
    }

    void OnClickOutPoint(ConnectionPoint out_point)
    {
        select_out_point = out_point;

        if (select_in_point != null)
        {
            if (select_out_point.node != select_in_point.node)
                CreateConnection(true);

            ClearConnectionSelection();
        }
    }

    //Function to delete combo connection
    void OnClickRemoveConnection(Connection c)
    {
        //Delete valid move and parent
        c.out_point.node.attack_object.valid_moves.Remove(c.in_point.node.attack_object);
        c.in_point.node.attack_object.parent = null;

        connections.Remove(c);
    }
    
    void CreateConnection(bool new_connection)
    {

        if(connections == null)
        {
            connections = new List<Connection>();
        }

        //Add pointers to eachother in attack object
        if(new_connection)
        {
            if (select_out_point.node.attack_object.valid_moves.Contains(select_in_point.node.attack_object))
                return;

            select_out_point.node.attack_object.valid_moves.Add(select_in_point.node.attack_object);
            select_in_point.node.attack_object.parent = select_out_point.node.attack_object;
        }

        connections.Add(new Connection(select_in_point, select_out_point, OnClickRemoveConnection));
    }

    void ClearConnectionSelection()
    {
        select_in_point = null;
        select_out_point = null;
    }
    

    /* HELPER FUNCTIONS THAT DRAW STUFF ON THE WINDOW */

    void DrawNodes()
    {
        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
                nodes[i].Draw();
        }
    }

    void DrawConnections()
    {
        if (connections != null)
            for (int i = 0; i < connections.Count; i++)
                connections[i].Draw();
    }

    private void DrawConnectionLine(Event e)
    {
        if (select_in_point != null && select_out_point == null)
        {
            Handles.DrawBezier(
                select_in_point.rect.center,
                e.mousePosition,
                select_in_point.rect.center + Vector2.left * 50f,
                e.mousePosition - Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }

        if (select_out_point != null && select_in_point == null)
        {
            Handles.DrawBezier(
                select_out_point.rect.center,
                e.mousePosition,
                select_out_point.rect.center - Vector2.left * 50f,
                e.mousePosition + Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;
        }
    }

    void DrawGrid(float grid_spacing, float grid_opacity, Color grid_color)
    {
        int width = Mathf.CeilToInt(position.width / grid_spacing);
        int height = Mathf.CeilToInt(position.height / grid_spacing);

        Handles.BeginGUI();
        Handles.color = new Color(grid_color.r, grid_color.g, grid_color.b, grid_opacity);

        offset += drag * 0.5f;
        Vector3 new_offset = new Vector3(offset.x % grid_spacing, offset.y % grid_spacing, 0.0f);

        //Drawing the lines
        for (int i = 0; i < width; i++)
            Handles.DrawLine(new Vector3(grid_spacing * i, -grid_spacing, 0.0f) + new_offset, new Vector3(grid_spacing * i, position.height, 0.0f) + new_offset);
        for (int i = 0; i < height; i++)
            Handles.DrawLine(new Vector3(-grid_spacing, grid_spacing * i, 0.0f) + new_offset, new Vector3(position.width, grid_spacing * i, 0.0f) + new_offset);

        Handles.color = Color.white;
        Handles.EndGUI();
    }
}
