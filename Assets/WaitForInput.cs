using UnityEngine;

public class WaitForInput : CustomYieldInstruction
{
    KeyCode chosenKey = KeyCode.None;
    bool firstframepassed = false;
    public override bool keepWaiting
    {
        get
        {
            if(!firstframepassed)
            {
                firstframepassed = true;
                return true;
            }
            if(chosenKey == KeyCode.None) return !Input.anyKeyDown;
            else return !Input.GetKeyDown(chosenKey);
        }
    }

    public WaitForInput(string id = "", KeyCode key = KeyCode.None){ chosenKey = key; Debug.Log("<b>"+ id +" Waiting for input " + key.ToString() + "...</b>"); }
}