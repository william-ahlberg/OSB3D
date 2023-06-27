using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public int seed; 
    public int blockCountX;
    public int blockCountZ; 

    public GameObject road;
    public GameObject crossing;
    //public GameObject block;
    public GameObject[] blocks;

    void Start()
    {
        //fixed numbers, the size of the 3d-assets
        float blockSize = 105;
        float roadWidth = 20; 

        AddBlocks(blockSize, roadWidth);
    }

    void AddBlocks(float _blockSize, float _roadWidth)
    {
        //adds to count for roads and crossings
        int loopX = blockCountX * 2 - 1;
        int loopZ = blockCountZ * 2 - 1;

        //how much position should be moved (in X/Z) for every placement
        float addToPos = _blockSize / 2 + _roadWidth / 2;

        //start placing at x=0, z=0 with the rotation of 0
        float currentPosX = 0;
        float currentPosZ = 0;
        float yRotation = 0;

        Random.InitState(seed);

        for (int i = 0; i < loopX; i++)
        {
            for (int j = 0; j < loopZ; j++)
            {
                GameObject newInstance;

                //if both even, instatiate a block
                if ((i % 2 == 0) && (j % 2 == 0))
                {
                    int chosen = Random.Range(0, blocks.Length);
                    newInstance = Instantiate(blocks[chosen], new Vector3(currentPosX, 0, currentPosZ), Quaternion.identity);

                    float rotateBlock = Random.Range(0, 4);
                    rotateBlock *= 90;
                    newInstance.transform.Rotate(0, rotateBlock, 0, Space.World);
                }

                //if both uneven, instantiate a crossing
                else if ((i % 2 != 0) && (j % 2 != 0))
                {

                    newInstance = Instantiate(crossing, new Vector3(currentPosX+_roadWidth/2, 0, currentPosZ+ _roadWidth / 2), Quaternion.identity);
                }

                //otherwise, instantiate a road
                else
                {
                    newInstance = Instantiate(road, new Vector3(currentPosX, 0, currentPosZ), Quaternion.identity);
                    newInstance.transform.Rotate(0, yRotation, 0, Space.World);
                }

                //placed all instances in the active game object
                newInstance.transform.parent = this.transform;

                currentPosZ += addToPos;
            }

            currentPosX += addToPos;
            currentPosZ = 0;

            //changes rotation every new x loop
            if (yRotation == 0) yRotation = 90;
            else yRotation = 0; 
        }
    }
}
