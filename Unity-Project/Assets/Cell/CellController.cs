using SharpNeat.Phenomes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitySharpNEAT;

public class CellController : UnitController
{
    public override float GetFitness()
    {
        Debug.Log("GetFitness");
        //throw new System.NotImplementedException();
        return 0;
    }

    protected override void HandleIsActiveChanged(bool newIsActive)
    {
        Debug.Log("HandleIsActiveChanged: " + newIsActive);
        //throw new System.NotImplementedException();
    }

    protected override void UpdateBlackBoxInputs(ISignalArray inputSignalArray)
    {
        Debug.Log("UpdateBlackBoxInputs: " + inputSignalArray);
        //throw new System.NotImplementedException();
    }

    protected override void UseBlackBoxOutpts(ISignalArray outputSignalArray)
    {
        Debug.Log("UseBlackBoxOutpts: " + outputSignalArray);
        //throw new System.NotImplementedException();
    }
}
