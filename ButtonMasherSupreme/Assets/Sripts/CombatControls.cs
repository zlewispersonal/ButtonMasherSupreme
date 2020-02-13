using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CombatControls : MonoBehaviour
{
    private AttackObject HeldAttack;
    private float InputDelay;
    private float ComboDropTimer;
    private float SpawnHitBoxTimer;
    private bool SpawnHitBox;
    //private bool DealtDamage;

    //Awak is called when the scene first boots up
    void Awake()
    {
        HeldAttack = null;
        InputDelay = 0.0f;
        ComboDropTimer = 0.0f;
        SpawnHitBoxTimer = 0.0f;
        SpawnHitBox = false;
        //DealtDamage = false;
    }

    //Update is called once per frame
    void Update()
    {
        CheckTimers();

        if(InputDelay <= 0.0f)
            GetInput();

        if (HeldAttack != null)
            DebugHitBox(Color.green);

        ProcessCollision();
    }

    void DebugHitBox(Color c)
    {
        Vector3 center = transform.position + new Vector3(HeldAttack.center_distance.x, HeldAttack.center_distance.y, 0.0f);
        Vector3 size = new Vector3(HeldAttack.size.x / 2, HeldAttack.size.y / 2, 0.0f);
        Debug.DrawLine(center - size, new Vector3(center.x - size.x, center.y + size.y, 0.0f), c);
        Debug.DrawLine(center + size, new Vector3(center.x - size.x, center.y + size.y, 0.0f), c);
        Debug.DrawLine(center - size, new Vector3(center.x + size.x, center.y - size.y, 0.0f), c);
        Debug.DrawLine(center + size, new Vector3(center.x + size.x, center.y - size.y, 0.0f), c);
    }

    //Function that goes through the timers and does actions
    void CheckTimers()
    {
        if (InputDelay > 0.0f)
            InputDelay -= Time.deltaTime;

        //Reset the combo, if dropped
        if (ComboDropTimer > 0.0f)
            ComboDropTimer -= Time.deltaTime;
        else
            HeldAttack = null;

        if (SpawnHitBoxTimer > 0.0f)
            SpawnHitBoxTimer -=  Time.deltaTime;
    }

    //Function to spawn a 2d ray to hit enemy
    void ProcessCollision()
    {
        //Go through held attack damage stuff
        if(SpawnHitBox && SpawnHitBoxTimer <= 0.0f)
        {
            SpawnHitBox = false;

            //Debug stuff
            DebugHitBox(Color.red);

            Collider2D enemy = Physics2D.OverlapBox(new Vector2(transform.position.x + HeldAttack.center_distance.x, 
                                                                transform.position.y + HeldAttack.center_distance.y), 
                                                    HeldAttack.size,
                                                    HeldAttack.angle,
                                                    LayerMask.GetMask("Enemy"));
            if (enemy != null)
            {
                Debug.Log("HIT");
                BasicBoss boss_health = enemy.GetComponent(typeof(BasicBoss)) as BasicBoss;
                boss_health.Health -= HeldAttack.damage;
                Debug.Log(boss_health.Health);
                //BasicBoss eh = enemy.transform.parent.gameObject.GetComponent(typeof(BasicBoss)) as BasicBoss;
                //eh.EnemyHealth -= HeldAttack.damage;
            }
            else
                Debug.Log("Miss");
        }
    }

    void GetInput()
    {
        if (Input.GetButtonDown("Attack1"))
        {
            ProcessAttack("Attack1");
        }
    }

    //Go through at continue combo if able
    void ProcessAttack(string attack)
    {
        if (HeldAttack == null)
        {
            FindStartCombo(attack);
            ApplyComboInfo();
        }
        else
        {
            //Go through valid moves apply the new HeldAttack if found
            if(ReadValidMoves(attack))
                ApplyComboInfo();
            else
            {
                FindStartCombo(attack);
                ApplyComboInfo();
            }
        }

        SpawnHitBox = true;
    }

    //Read through the valid moves in heldattack to see if attack is valid
    bool ReadValidMoves(string attack)
    {
        foreach(AttackObject a in HeldAttack.valid_moves)
        {
            if (a.button == attack)
            {
                HeldAttack = a;
                return true;
            }
        }
        return false;
    }

    //Special function to find start to a combo
    void FindStartCombo(string attack)
    {
        if (attack == "Attack1")
            HeldAttack = (AttackObject)AssetDatabase.LoadAssetAtPath("Assets/Attack Objects/Combo_Base_1.asset", typeof(AttackObject));
    }

    //Reads through the object and applies all details
    void ApplyComboInfo()
    {
        ComboDropTimer = HeldAttack.combo_drop_timer;
        InputDelay = HeldAttack.input_delay;
        SpawnHitBoxTimer = HeldAttack.spawn_hitbox_timer;
    }
}
