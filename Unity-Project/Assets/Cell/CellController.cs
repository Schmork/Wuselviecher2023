using SharpNeat.Phenomes;
using System.Collections.Generic;
using UnityEngine;
using UnitySharpNEAT;

public class CellController : UnitController
{
    SizeController sc;
    SensorController sensors;
    Rigidbody2D rb;
    MovementController mc;

    private void Start()
    {
        sc = GetComponent<SizeController>();
        sensors = GetComponent<SensorController>();
        rb = GetComponent<Rigidbody2D>();
        mc = GetComponent<MovementController>();
    }

    public override float GetFitness()
    {
        //Debug.Log("GetFitness");
        //throw new System.NotImplementedException();
        return 0;
    }

    protected override void HandleIsActiveChanged(bool newIsActive)
    {
        //Debug.Log("HandleIsActiveChanged: " + newIsActive);
        //throw new System.NotImplementedException();
    }

    protected override void UpdateBlackBoxInputs(ISignalArray inputSignalArray)
    {
        if (BlackBox == null) return;

        var inputList = new List<float> { sc.Size / 500f, rb.velocity.magnitude / 10f, rb.angularVelocity / 50f };
        // memNeurons
        inputList.AddRange(sensors.Scan());

        for (int i = 0; i < inputSignalArray.Length; i++)
        {
            inputSignalArray[i] = inputList[i];
        }

        //Debug.Log("UpdateBlackBoxInputs: " + inputSignalArray);
    }

    protected override void UseBlackBoxOutpts(ISignalArray outputSignalArray)
    {
        if (BlackBox == null) return;

        mc.UseBlackBoxOutpts(outputSignalArray);

        //Debug.Log("UseBlackBoxOutpts: " + outputSignalArray);
    }
}
