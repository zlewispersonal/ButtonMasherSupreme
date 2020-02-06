using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackControls : MonoBehaviour
{
    private const uint MAX_SIZE = 5;
    
    //Timer Reset Fiesta
    private const float RESET_COMBO_TIMER = 1.5f;
    private const float WAIT_INPUT_TIMPER = 0.15f;
    private const float XXX_COMBO_TIMER = 0.5f;

    private List<string> Combo = new List<string>();
    private float CurrentTimer;
    private float InputTimer;
    private float AnimationTimer;

    void Awake()
    {
        CurrentTimer = 0.0f;
        InputTimer = 0.0f;
        AnimationTimer = 0.0f;
    }

    void Update()
    {
        if(WaitForInput())
            GetInput();
        if (GlobalComboResetTimer())
            AttackFlow();
        CheckResetCombo();
    }

    //A global reset timer for certain combos, so no spamming
    bool GlobalComboResetTimer()
    {
        if (AnimationTimer > 0.0f)
        {
            AnimationTimer -= Time.deltaTime;
            return false;
        }

        return true;
    }

    //Give a delay so no button smashers
    bool WaitForInput()
    {
        if (InputTimer > 0.0f)
        {
            InputTimer -= Time.deltaTime;
            return false;
        }

        return true;
    }

    //Basic logic to get input
    void GetInput()
    {
        List<string> reader = new List<string>();
        
        if (Input.GetButtonDown("Attack1"))
            reader.Add("x");

        if (reader.Count > 0)
        {
            InputTimer = WAIT_INPUT_TIMPER;
            ParseButtons(ref reader);
        }
    }

    void CheckResetCombo()
    {
        //If there is anything being held in combo
        if (Combo.Count > 0.0f)
            CurrentTimer -= Time.deltaTime;

        //Reset the combo if time runs out
        if (CurrentTimer <= 0.0f)
            Combo.Clear();
    }

    //Try to find out what the player wants
    void ParseButtons(ref List<string> keys)
    {
        CurrentTimer = RESET_COMBO_TIMER;
        if (Combo.Count == MAX_SIZE)
            Combo.RemoveAt(0);

        if (keys.Count > 1)
        {
            Combo.Add(keys[keys.Count - 1] + "+" + keys[keys.Count - 2]);
        }
        else
            Combo.Add(keys[0]);
    }

    //Long logic to check which combo
    void AttackFlow()
    {
        int combo_num = Combo.Count;

        //FIRST HIT STUFF
        if (combo_num == 1)
        {
            //X
            if (Combo[combo_num - 1] == "x")
            {
                //Spawn Sprite
            }
        }
        else if(combo_num == 2)
        {
            //X X
            if (Combo[combo_num - 1] == "x" && Combo[combo_num - 2] == "x")
            {
                //Spawn Sprite
            }
        }
        else if (combo_num == 3)
        {
            //X X X
            if (Combo[combo_num - 1] == "x" && Combo[combo_num - 2] == "x" && Combo[combo_num - 3] == "x")
            {
                //Spawn Sprite

                AnimationTimer = XXX_COMBO_TIMER;
            }
        }
    }
}
