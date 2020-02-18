using System;
using UnityEngine;
using UnityEditor;

public class ComboNode
{
    public Rect rect;
    public string title;
    public bool is_dragged;
    public AttackObject attack_object;

    public ConnectionPoint in_point;
    public ConnectionPoint out_point;

    public GUIStyle style;

    public Action<ComboNode> OnRemoveNode;

    public ComboNode(Vector2 position, 
                     float width,
                     float height,
                     GUIStyle nodeStyle,
                     GUIStyle in_style,
                     GUIStyle out_style,
                     Action<ConnectionPoint> OnClickInPoint,
                     Action<ConnectionPoint> OnClickOutPoint,
                     Action<ComboNode> OnclickdeRemoveNode,
                     AttackObject attack)
    {
        rect = new Rect(position.x, position.y, width, height);
        style = nodeStyle;
        in_point = new ConnectionPoint(this, ConnectionPointType.In, in_style, OnClickInPoint);
        out_point = new ConnectionPoint(this, ConnectionPointType.Out, out_style, OnClickOutPoint);
        OnRemoveNode = OnclickdeRemoveNode;
        attack_object = attack;
    }

    public void Drag(Vector2 delta)
    {
        rect.position += delta;
    }

    public void Draw()
    {
        in_point.Draw();
        out_point.Draw();
        //GUI.Box(rect, title, style);
        GUILayout.BeginArea(rect, style);
        GUILayout.Box("Attack Object");

        //NAME
        GUILayout.BeginHorizontal();
        GUILayout.Label("Name:");
        string new_name = EditorGUILayout.TextField(attack_object.name);
        AssetDatabase.RenameAsset("Assets/Attack Objects/" + attack_object.name + ".asset", new_name);
        attack_object.name = new_name;
        GUILayout.EndHorizontal();

        //BUTTON
        GUILayout.BeginHorizontal();
        GUILayout.Label("Button:");
        attack_object.button = EditorGUILayout.TextField(attack_object.button);
        GUILayout.EndHorizontal();

        //SPRITE
        GUILayout.BeginHorizontal();
        GUILayout.Label("Sprite:");
        attack_object.attack_animation = (Sprite)EditorGUILayout.ObjectField(attack_object.attack_animation, typeof(Sprite), allowSceneObjects: true);
        GUILayout.EndHorizontal();

        //COMBO DROP TIMER
        GUILayout.BeginHorizontal();
        GUILayout.Label("Combo Drop Timer:");
        attack_object.combo_drop_timer = EditorGUILayout.FloatField(attack_object.combo_drop_timer);
        GUILayout.EndHorizontal();

        //INPUT DELAY
        GUILayout.BeginHorizontal();
        GUILayout.Label("Input Delay Timer:");
        attack_object.input_delay = EditorGUILayout.FloatField(attack_object.input_delay);
        GUILayout.EndHorizontal();

        //SPAWN HITBOX TIMER
        GUILayout.BeginHorizontal();
        GUILayout.Label("Hitbox Spawn Timer:");
        attack_object.spawn_hitbox_timer = EditorGUILayout.FloatField(attack_object.spawn_hitbox_timer);
        GUILayout.EndHorizontal();

        //DAMAGE
        GUILayout.BeginHorizontal();
        GUILayout.Label("Damage:");
        attack_object.damage = EditorGUILayout.IntField(attack_object.damage);
        GUILayout.EndHorizontal();

        //COLLIDER TYPE
        GUILayout.BeginHorizontal();
        int selected = 0;
        if (attack_object.collider_type == AttackObject.ColliderType.Circle)
            selected = 0;
        else if (attack_object.collider_type == AttackObject.ColliderType.Box)
            selected = 1;

        string[] collider_options = new string[]
        {
            "Circle", "Box"
        };
        GUILayout.Label("Collider Type:");
        selected = EditorGUILayout.Popup(selected, collider_options);

        if (selected == 0)
            attack_object.collider_type = AttackObject.ColliderType.Circle;
        else if (selected == 1)
            attack_object.collider_type = AttackObject.ColliderType.Box;
        GUILayout.EndHorizontal();

        //CENTER DISTANCE
        GUILayout.Label("Center Distance:");
        GUILayout.BeginHorizontal();
        Vector2 new_dist = new Vector2();

        GUILayout.Label("X");
        new_dist.x = EditorGUILayout.FloatField(attack_object.center_distance.x);
        GUILayout.Label("Y");
        new_dist.y = EditorGUILayout.FloatField(attack_object.center_distance.y);

        attack_object.center_distance = new_dist;
        GUILayout.EndHorizontal();


        if (attack_object.collider_type == AttackObject.ColliderType.Box)
        {
            //BOX SIZE
            GUILayout.Label("Box Size:");
            GUILayout.BeginHorizontal();
            Vector2 new_size = new Vector2();

            GUILayout.Label("X");
            new_dist.x = EditorGUILayout.FloatField(attack_object.size.x);
            GUILayout.Label("Y");
            new_dist.y = EditorGUILayout.FloatField(attack_object.size.y);

            attack_object.size = new_size;
            GUILayout.EndHorizontal();

            //BOX ANGLE
            GUILayout.BeginHorizontal();
            GUILayout.Label("Box Angle:");
            attack_object.angle = EditorGUILayout.FloatField(attack_object.angle);
            GUILayout.EndHorizontal();
        }
        else if(attack_object.collider_type == AttackObject.ColliderType.Circle)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Circle Radius:");
            attack_object.radius = EditorGUILayout.FloatField(attack_object.radius);
            GUILayout.EndHorizontal();
        }


        GUILayout.EndArea();
    }

    public bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if (rect.Contains(e.mousePosition))
                    {
                        is_dragged = true;
                        GUI.changed = true;
                    }
                    else
                    {
                        GUI.changed = true;
                    }
                }

                if(e.button == 1 && rect.Contains(e.mousePosition))
                {
                    ProcessContextMenu();
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                is_dragged = false;
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && is_dragged)
                {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }

        return false;
    }

    void ProcessContextMenu()
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
        genericMenu.ShowAsContext();
    }

    void OnClickRemoveNode()
    {
        if (OnRemoveNode != null)
            OnRemoveNode(this);
    }
}
