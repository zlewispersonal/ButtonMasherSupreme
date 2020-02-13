using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "Attack Objects")]
public class AttackObject : ScriptableObject
{
    public string button;
    public List<AttackObject> valid_moves;
    public Sprite attack_animation;
    public float combo_drop_timer;
    public float input_delay;
    public float spawn_hitbox_timer;
    public int damage;

    //hitbox info
    public Vector2 center_distance;
    public Vector2 size;
    public float angle;

    /*
    //Collider junk
    public enum ColliderType
    {
        Circle,
        Box
    };
    public ColliderType collider_type;
    */
}
