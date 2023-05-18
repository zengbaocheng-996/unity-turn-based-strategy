using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    [SerializeField] private Unit unit;
    //[SerializeField] private Transform gridDebugObjectPrefab;
    //private GridSystem gridSystem;
    private void Start()
    {
        //gridSystem=new GridSystem(10,10,2f);
        //gridSystem.CreateDebugObjects(gridDebugObjectPrefab);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            
        }
        //Debug.Log(gridSystem.GetGridPosition(MouseWorld.GetPosition()));
    }
}
