using System;
using UnityEditor;
using UnityEngine;

public class Connection
{
    public ConnectionPoint in_point;
    public ConnectionPoint out_point;
    public Action<Connection> OnClickRemoveConnection;

    public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint, Action<Connection> OnClickRemoveConnection)
    {
        this.in_point = inPoint;
        this.out_point = outPoint;
        this.OnClickRemoveConnection = OnClickRemoveConnection;
    }

    public void Draw()
    {
        Handles.DrawBezier(
            in_point.rect.center,
            out_point.rect.center,
            in_point.rect.center + Vector2.left * 50f,
            out_point.rect.center - Vector2.left * 50f,
            Color.white,
            null,
            2f
        );

        if (Handles.Button((in_point.rect.center + out_point.rect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
        {
            if (OnClickRemoveConnection != null)
            {
                OnClickRemoveConnection(this);
            }
        }
    }
}
