using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateObject : MonoBehaviour {

    static float minX = -3.5f, maxX = 3.5f, minY = 0.5f, maxY = 9.5f;

    // astatic function for converting position of cubes to index on tetris map
    public static Vector2Int GetIndex(Vector3 p)
    {
        int idx = Mathf.RoundToInt(p.x + 3.51f);
        int idy = Mathf.RoundToInt(p.y - 0.49f);

        return new Vector2Int(idx, idy);
    }

    public static string FromBlockTagsToString(BlockTags bt)
    {
        switch (bt)
        {
            case BlockTags.Block:
                return "Block";
            case BlockTags.Ground:
                return "Ground";
            case BlockTags.LeftWall:
                return "LeftWall";
            case BlockTags.MovingBlock:
                return "MovingBlock";
            case BlockTags.RightWall:
                return "RightWall";
        }

        return "";
    }

    public enum ObjectType { ObjL = 0, ObjI = 1, ObjO = 2};
    public enum BlockTags { Block = 7, RightWall = 8, LeftWall = 9, Ground = 10, MovingBlock = 11};

    public ObjectType curObjType = ObjectType.ObjI;
    public BlockTags curObjTag = BlockTags.MovingBlock;

    public bool flagCreateOne = true; // a command to create a new block of objects
    public float objSize = 3.0f; // size of the objects, meaning 3 cubes
    public Vector2 OriginFrom; // random init postion of block
    public Vector2 OriginTo; // random init postion of block
    [HideInInspector] public ArrayList AllObjs; // all cubes for moving objects
    [HideInInspector] public GameObject handleObj = null; // handle to parent object for moving, rotating of the block of cubes
    BlockObjProperty mHandleObjPropertyHandler = null; // handle to property of handle object
    ControlEnvironment mHandlerControlEnv = null; // handle to control environment class

    // Use this for initialization
    void Start ()
    {
        AllObjs = new ArrayList();
        handleObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Destroy(handleObj.GetComponent<BoxCollider>());
        handleObj.transform.SetParent(this.transform);
        handleObj.GetComponent<Renderer>().enabled = false;
        handleObj.AddComponent<BlockObjProperty>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (mHandlerControlEnv == null)
            mHandlerControlEnv = handleObj.transform.parent.GetComponent<ControlEnvironment>();
        if (mHandleObjPropertyHandler == null && handleObj != null)
            mHandleObjPropertyHandler = handleObj.GetComponent<BlockObjProperty>();

        if (flagCreateOne)
        {
            MakeObject();
        }

        CheckForBoundaries();
    }

    // check that all cubes in the object is inside the tetris environment
    void CheckForBoundaries()
    {
        if (this.tag != "MovingBlock")
            return;

        for (int i = 0; i < AllObjs.Count; i++)
        {
            Vector3 p = ((GameObject)AllObjs[i]).transform.position;

            Vector2Int id = CreateObject.GetIndex(p);

            ((GameObject)AllObjs[i]).transform.position = new Vector3(id.x - 3.5f, id.y + 0.5f, 0.0f);
        }

        Vector2 MinBoundaries = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 MaxBoundaries = new Vector2(float.MinValue, float.MinValue);
        
        for (int i = 0; i < AllObjs.Count; i++)
        {
            Vector3 p = ((GameObject)AllObjs[i]).transform.position;

            if (p.x < MinBoundaries.x)
            {
                MinBoundaries = new Vector2(p.x, MinBoundaries.y);
            }
            if (p.y < MinBoundaries.y)
            {
                MinBoundaries = new Vector2(MinBoundaries.x, p.y);
            }

            if (p.x > MaxBoundaries.x)
            {
                MaxBoundaries = new Vector2(p.x, MaxBoundaries.y);
            }
            if (p.y > MaxBoundaries.y)
            {
                MaxBoundaries = new Vector2(MaxBoundaries.x, p.y);
            }
        }

        if (MinBoundaries.y < minY)
        {
            float diff = minY - MinBoundaries.y;
            handleObj.transform.position += new Vector3(0.0f, diff, 0.0f);
            mHandleObjPropertyHandler.canGoDown = false;
        }

        if (MaxBoundaries.y > maxY)
        {
            float diff = maxY - MaxBoundaries.y;
            handleObj.transform.position += new Vector3(0.0f, diff, 0.0f);
        }

        if (MinBoundaries.x < minX)
        {
            float diff = minX - MinBoundaries.x;
            handleObj.transform.position += new Vector3(diff, 0.0f, 0.0f);
        }

        if (MaxBoundaries.x > maxX)
        {
            float diff = maxX - MaxBoundaries.x;
            handleObj.transform.position += new Vector3(diff, 0.0f, 0.0f);
        }

        return;
    }

    public void MakeObject()
    {
        switch (curObjType)
        {
            case ObjectType.ObjI:
                MakeIShape();
                break;
            case ObjectType.ObjL:
                MakeLShape();
                break;
            case ObjectType.ObjO:
                MakeOShape();
                break;
            default:
                MakeIShape();
                break;
        }

        Vector2 diff = OriginTo - OriginFrom;
        handleObj.transform.position = new Vector3(OriginFrom.x, OriginFrom.y, 0.0f)
            + Random.Range(0.0f, 1.0f) * new Vector3(diff.x, diff.y, 0.0f);

        flagCreateOne = false;
        CheckForBoundaries();
    }

    private void MakeIShape()
    {
        for (float i = -objSize / 2.0f; i < objSize / 2.0f; i += 1.0f)
        {
            GameObject nObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            nObj.transform.SetParent(handleObj.transform);
            nObj.transform.localPosition = new Vector3(0.5f, i + 0.5f, 0);
            nObj.transform.tag = CreateObject.FromBlockTagsToString(curObjTag);
            BlockCollisionHandle tmp = nObj.AddComponent<BlockCollisionHandle>();
            tmp.handleObj = handleObj;
            Rigidbody tmp_rb = nObj.AddComponent<Rigidbody>();
            tmp_rb.constraints = RigidbodyConstraints.FreezeAll;

            AllObjs.Add(nObj);
        }

        return;
    }

    private void MakeLShape()
    {
        for (int i = 0; i < objSize; i++)
        {
            GameObject nObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            nObj.transform.SetParent(handleObj.transform);
            nObj.transform.localPosition = new Vector3(0.5f, i + 0.5f, 0);
            nObj.transform.tag = CreateObject.FromBlockTagsToString(curObjTag);
            BlockCollisionHandle tmp = nObj.AddComponent<BlockCollisionHandle>();
            tmp.handleObj = handleObj;
            Rigidbody tmp_rb = nObj.AddComponent<Rigidbody>();
            tmp_rb.constraints = RigidbodyConstraints.FreezeAll;

            AllObjs.Add(nObj);
        }

        for (int i = 1; i < objSize - 1; i++)
        {
            GameObject nObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            nObj.transform.SetParent(handleObj.transform);
            nObj.transform.localPosition = new Vector3(0.5f + i, 0.5f, 0);
            nObj.transform.tag = CreateObject.FromBlockTagsToString(curObjTag);
            BlockCollisionHandle tmp = nObj.AddComponent<BlockCollisionHandle>();
            tmp.handleObj = handleObj;
            Rigidbody tmp_rb = nObj.AddComponent<Rigidbody>();
            tmp_rb.constraints = RigidbodyConstraints.FreezeAll;

            AllObjs.Add(nObj);
        }

        return;
    }

    private void MakeOShape()
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                GameObject nObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

                nObj.transform.SetParent(handleObj.transform);
                nObj.transform.localPosition = new Vector3(0.5f + i, 0.5f + j, 0);
                nObj.transform.tag = CreateObject.FromBlockTagsToString(curObjTag);
                BlockCollisionHandle tmp = nObj.AddComponent<BlockCollisionHandle>();
                tmp.handleObj = handleObj;
                Rigidbody tmp_rb = nObj.AddComponent<Rigidbody>();
                tmp_rb.constraints = RigidbodyConstraints.FreezeAll;

                AllObjs.Add(nObj);
            }
        }
        return;
    }

    // when object is not moving, the cubes are copied in the tetris and removed from handle obj
    public void ResetHandleObj()
    {
        mHandleObjPropertyHandler.canGoDown = true;
        mHandleObjPropertyHandler.canGoLeft = true;
        mHandleObjPropertyHandler.canGoRight = true;

        AllObjs.Clear();
    }

    public void MoveRight()
    {
        if (mHandlerControlEnv == null)
            mHandlerControlEnv = handleObj.transform.parent.GetComponent<ControlEnvironment>();

        // check if the object can go right
        mHandleObjPropertyHandler.canGoRight = true;
        for (int i = 0; i < AllObjs.Count && mHandleObjPropertyHandler.canGoRight; i++)
        {
            Vector3 p = ((GameObject)AllObjs[i]).transform.position;

            Vector2Int id = CreateObject.GetIndex(p);

            if (id.x >= 7)
            {
                mHandleObjPropertyHandler.canGoRight = false;
                break;
            }
            if (mHandlerControlEnv.tetrisMap[id.y][id.x + 1] == 1)
            {
                mHandleObjPropertyHandler.canGoRight = false;
                break;
            }
        }

        if (mHandleObjPropertyHandler.canGoRight)
            handleObj.transform.position += new Vector3(+1, 0, 0);
        CheckForBoundaries();
    }

    public void MoveLeft()
    {
        if (mHandlerControlEnv == null)
            mHandlerControlEnv = handleObj.transform.parent.GetComponent<ControlEnvironment>();

        // check if the object can go left
        mHandleObjPropertyHandler.canGoLeft = true;
        for (int i = 0; i < AllObjs.Count && mHandleObjPropertyHandler.canGoLeft; i++)
        {
            Vector3 p = ((GameObject)AllObjs[i]).transform.position;

            Vector2Int id = CreateObject.GetIndex(p);

            if (id.x <= 0)
            {
                mHandleObjPropertyHandler.canGoLeft = false;
                break;
            }
            if (mHandlerControlEnv.tetrisMap[id.y][id.x - 1] == 1)
            {
                mHandleObjPropertyHandler.canGoLeft = false;
                break;
            }
        }

        if (mHandleObjPropertyHandler.canGoLeft)
            handleObj.transform.position += new Vector3(-1, 0, 0);
        CheckForBoundaries();
    }

    public bool MoveDown()
    {
        if (mHandlerControlEnv == null)
            mHandlerControlEnv = handleObj.transform.parent.GetComponent<ControlEnvironment>();

        // check if the object can go down
        mHandleObjPropertyHandler.canGoDown = true;
        for (int i = 0; i < AllObjs.Count && mHandleObjPropertyHandler.canGoDown; i++)
        {
            Vector3 p = ((GameObject)AllObjs[i]).transform.position;

            Vector2Int id = CreateObject.GetIndex(p);

            if (id.y <= 0)
            {
                mHandleObjPropertyHandler.canGoDown = false;
                break;
            }
            if (mHandlerControlEnv.tetrisMap[id.y - 1][id.x] == 1)
            {
                mHandleObjPropertyHandler.canGoDown = false;
                break;
            }
        }

        if (mHandleObjPropertyHandler.canGoDown)
        {
            handleObj.transform.position += new Vector3(0, -1, 0);
            return true;
        }
        return false;
    }

    public void Rotate()
    {
        if (mHandlerControlEnv == null)
            mHandlerControlEnv = handleObj.transform.parent.GetComponent<ControlEnvironment>();

        Vector3 old_euler = handleObj.transform.rotation.eulerAngles;
        Vector3 new_euler = old_euler + new Vector3(0, 0, 90);
        handleObj.transform.rotation = Quaternion.Euler(new_euler);

        // check if the object can rotate
        bool flag_colide = false;
        for (int i = 0; i < AllObjs.Count && !flag_colide; i++)
        {
            Vector3 p = ((GameObject)AllObjs[i]).transform.position;

            Vector2Int id = CreateObject.GetIndex(p);

            if (id.x >= 0 && id.x < mHandlerControlEnv.tetrisMap[0].Length && id.y >= 0 && id.y < mHandlerControlEnv.tetrisMap.Length)
            {
                if (mHandlerControlEnv.tetrisMap[id.y][id.x] == 1)
                {
                    flag_colide = true;
                    break;
                }
            }
            else
            {
                flag_colide = true;
                break;
            }
        }

        // do not rotate if it is not possible
        if (flag_colide)
        {
            handleObj.transform.rotation = Quaternion.Euler(old_euler);
        }
        CheckForBoundaries();
    }
}
