using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlEnvironment : MonoBehaviour {

    CreateObject mObjectCreatorHandler; // a handler to create object class, sending command to the block of cubes to move and be created
    float current_time = 0; // current time that moving down happened, keep changing every second

    int nRows = 10; // number of rows in tetris
    int nCols = 8; // number of columns in tetris

    KeyCode completedAction = KeyCode.None; // current user's command that has been down, preventing user from holding the key for applying command

    public int[][] tetrisMap; // tetris map of having cubes
    GameObject[][] tetrisMapGameObj; // access to the cubes in the tetris environment
    int[] sumRowsTetris; // keep the sum of cubes in each row of the tetris, used to remove a row
    bool flag_reset_all = false; // if a new object comes on the already written block, reset the game (game over)

    // Use this for initialization
    void Start ()
    {
        mObjectCreatorHandler = GetComponent<CreateObject>();
        current_time = Time.realtimeSinceStartup;

        tetrisMap = new int[nRows][];
        tetrisMapGameObj = new GameObject[nRows][];
        for (int i = 0; i < nRows; i++)
        {
            tetrisMap[i] = new int[nCols];
            tetrisMapGameObj[i] = new GameObject[nCols];
        }
        sumRowsTetris = new int[nRows];
    }
    
	// Update is called once per frame
	void Update ()
    {
        // if the elapsed time is 1.0 second
        if (Time.realtimeSinceStartup - current_time >= 1.0f)
        {
            if (!mObjectCreatorHandler.MoveDown())
            {
                CopyObjAsBlocks();
                mObjectCreatorHandler.curObjType = (CreateObject.ObjectType)(Random.Range((int)0, (int)3));
                mObjectCreatorHandler.flagCreateOne = true;
            }
            current_time = Time.realtimeSinceStartup;
        }

        if (Input.GetKey(KeyCode.RightArrow) && completedAction != KeyCode.RightArrow)
        {
            mObjectCreatorHandler.MoveRight();
            completedAction = KeyCode.RightArrow;
        }
        if (Input.GetKeyUp(KeyCode.RightArrow) && completedAction == KeyCode.RightArrow)
        {
            completedAction = KeyCode.None;
        }

        if (Input.GetKey(KeyCode.LeftArrow) && completedAction != KeyCode.LeftArrow)
        {
            mObjectCreatorHandler.MoveLeft();
            completedAction = KeyCode.LeftArrow;
        }
        if (Input.GetKeyUp(KeyCode.LeftArrow) && completedAction == KeyCode.LeftArrow)
        {
            completedAction = KeyCode.None;
        }

        if (Input.GetKey(KeyCode.DownArrow) && completedAction != KeyCode.DownArrow)
        {
            mObjectCreatorHandler.MoveDown();
            completedAction = KeyCode.DownArrow;
        }
        if (Input.GetKeyUp(KeyCode.DownArrow) && completedAction == KeyCode.DownArrow)
        {
            completedAction = KeyCode.None;
        }

        if (Input.GetKey(KeyCode.Space) && completedAction != KeyCode.Space)
        {
            mObjectCreatorHandler.Rotate();
            completedAction = KeyCode.Space;
        }
        if (Input.GetKeyUp(KeyCode.Space) && completedAction == KeyCode.Space)
        {
            completedAction = KeyCode.None;
        }

        RemoveCompletedRows();

        ResetAll();
    }
    
    // once the block of cubes stops moving copy them to the tetris, and change the map, objects and sum row accordingly
    void CopyObjAsBlocks()
    {
        // y = 0.5 to 9.5 -> idy = 0 ... 9
        // x = -3.5 to 3.5 -> idx = 0 ... 7

        for (int o = 0; o < mObjectCreatorHandler.AllObjs.Count; o++)
        {
            Vector3 p = ((GameObject)mObjectCreatorHandler.AllObjs[o]).transform.position;

            Vector2Int id = CreateObject.GetIndex(p);

            if (id.y >= 0 && id.y < nRows && id.x >= 0 && id.x < nCols)
            {
                if (tetrisMap[id.y][id.x] == 0)
                    sumRowsTetris[id.y] += 1;
                else
                {
                    flag_reset_all = true;
                }
                tetrisMap[id.y][id.x] = 1;
            }
            tetrisMapGameObj[id.y][id.x] = (GameObject)mObjectCreatorHandler.AllObjs[o];

            tetrisMapGameObj[id.y][id.x].transform.SetParent(this.transform);
            tetrisMapGameObj[id.y][id.x].transform.tag = CreateObject.FromBlockTagsToString(CreateObject.BlockTags.Block);
            tetrisMapGameObj[id.y][id.x].GetComponent<Renderer>().material.color = Color.black;
        }

        mObjectCreatorHandler.ResetHandleObj();
    }

    // remove the completed row, give command to other block to move down accordingly
    void RemoveCompletedRows()
    {
        // find the rows that can move down, and not getting removed
        ArrayList valid_rows = new ArrayList();
        for (int i = 0; i < nRows; i++)
        {
            if (sumRowsTetris[i] < nCols && sumRowsTetris[i] > 0)
            {
                valid_rows.Add(i);
            }
        }

        int current_index_on_valid_rows = 0;
        int current_removed_rows = 0;
        for (int i = 0; i < nRows; i++)
        {
            // find the row that can be replaced on the removed one, so if one get removed, we move the exisitng one on the top of it
            int replace_row_idx = -1;
            if (valid_rows.Count > 0 && current_index_on_valid_rows < valid_rows.Count)
            {
                replace_row_idx = (int)valid_rows[current_index_on_valid_rows];
                while (replace_row_idx <= i && current_index_on_valid_rows < valid_rows.Count)
                {
                    current_index_on_valid_rows++;
                    if (current_index_on_valid_rows < valid_rows.Count)
                        replace_row_idx = (int)valid_rows[current_index_on_valid_rows];
                }

                if (replace_row_idx <= i)
                    replace_row_idx = -1;
            }

            // removing the completed rows, and move existing ones
            if (sumRowsTetris[i] >= nCols || current_removed_rows > 0)
            {
                current_removed_rows++;
                for (int j = 0; j < nCols; j++)
                {
                    if (tetrisMapGameObj[i][j] != null)
                    {
                        Destroy(tetrisMapGameObj[i][j]);
                        tetrisMapGameObj[i][j] = null;
                    }
                    if (replace_row_idx > -1)
                    {
                        tetrisMapGameObj[i][j] = tetrisMapGameObj[replace_row_idx][j];
                        tetrisMapGameObj[replace_row_idx][j] = null;

                        tetrisMap[i][j] = tetrisMap[replace_row_idx][j];
                        tetrisMap[replace_row_idx][j] = 0;

                        if (tetrisMapGameObj[i][j] != null)
                            tetrisMapGameObj[i][j].GetComponent<BlockCollisionHandle>().num_tiles_to_go_down = replace_row_idx - i;
                    }
                    else
                    {
                        tetrisMapGameObj[i][j] = null;
                        tetrisMap[i][j] = 0;
                    }
                }
                
                if (replace_row_idx > -1)
                {
                    sumRowsTetris[i] = sumRowsTetris[replace_row_idx];
                    sumRowsTetris[replace_row_idx] = 0;
                }
                else
                {
                    sumRowsTetris[i] = 0;
                }

                current_index_on_valid_rows++;
            }
        }

        return;
    }

    // if game is over, reset all variables. happens when a new block overwrite an already written block
    void ResetAll()
    {
        if (!flag_reset_all)
            return;
        for (int i = 0; i < nRows; i++)
        {
            for (int j = 0; j < nCols; j++)
            {
                tetrisMap[i][j] = 0;
                if (tetrisMapGameObj[i][j] != null)
                {
                    Destroy(tetrisMapGameObj[i][j]);
                }
            }
            sumRowsTetris[i] = 0;
        }

        Transform[] ts = this.GetComponentsInChildren<Transform>();
        for (int i = 0; i < ts.Length; i++)
        {
            if (ts[i].tag == "Block")
            {
                Destroy(ts[i].gameObject);
            }
        }

        mObjectCreatorHandler.ResetHandleObj();
        flag_reset_all = false;
    }
}
