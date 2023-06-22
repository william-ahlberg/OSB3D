using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public int blockCountX;
    public int blockCountZ; 

    public GameObject road;
    public GameObject crossing;
    public GameObject block; 

    void Start()
    {
        float blockSize = 105;
        float roadWidth = 20; 

        AddBlocks(blockSize, roadWidth);
    }

    void AddBlocks(float _blockSize, float _roadWidth)
    {
        //adds to count for roads and crossings
        int loopX = blockCountX * 2 - 1;
        int loopZ = blockCountZ * 2 - 1;

        float currentPosX = 0;
        float currentPosZ = 0;

        float addToPos = _blockSize / 2 + _roadWidth / 2;


        GameObject newInstance;
        float yRotation = 0; 

        for (int i = 0; i < loopX; i++)
        {

            for (int j = 0; j < loopZ; j++)
            {

                if ((i % 2 == 0) && (j % 2 == 0))
                {
                    newInstance = Instantiate(block, new Vector3(currentPosX, 0, currentPosZ), Quaternion.identity);
                }

                else if ((i % 2 != 0) && (j % 2 != 0))
                {
                    newInstance = Instantiate(crossing, new Vector3(currentPosX+_roadWidth/2, 0, currentPosZ+ _roadWidth / 2), Quaternion.identity);
                }

                else
                {
                    newInstance = Instantiate(road, new Vector3(currentPosX, 0, currentPosZ), Quaternion.identity);
                    newInstance.transform.Rotate(0, yRotation, 0, Space.World);
                }

                newInstance.transform.parent = this.transform;

                currentPosZ += addToPos;
            }

            currentPosX += addToPos;
            currentPosZ = 0;

            if (yRotation == 0) yRotation = 90;
            else yRotation = 0; 
        }
    }
}
