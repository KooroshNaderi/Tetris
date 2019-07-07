using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCollisionHandle : MonoBehaviour {

    public GameObject handleObj;

    public int num_tiles_to_go_down = 0;
    float current_time = 0;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        // handling each block for its own if it has a command to move
        if (Time.realtimeSinceStartup - current_time >= 0.25f)
        {
            current_time = Time.realtimeSinceStartup;
            MoveDown();
        }
        CheckForBoundaries();
    }

    void MoveDown()
    {
        if (this.tag == "Block" && num_tiles_to_go_down > 0)
        {
            this.transform.position += new Vector3(0, -1, 0);
            num_tiles_to_go_down--;
        }
    }

    void CheckForBoundaries()
    {
        if (this.tag == "Block")
        {
            if (this.transform.position.x <= -3.5f)
            {
                this.transform.position = new Vector3(-3.5f, this.transform.position.y, 0.0f);
            }

            if (this.transform.position.x >= 3.5f)
            {
                this.transform.position = new Vector3(3.5f, this.transform.position.y, 0.0f);
            }

            if (this.transform.position.y <= 0.5f)
            {
                this.transform.position = new Vector3(this.transform.position.x, 0.5f, 0.0f);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (this.tag == "MovingBlock")
        {
            if (collision.transform.tag == "Block")
            {
                ControlEnvironment tmp_handler = collision.transform.parent.GetComponent<ControlEnvironment>();
                Vector3 p = this.transform.position;

                Vector2Int id = CreateObject.GetIndex(p);

                if (id.y - 1 >= 0 && tmp_handler.tetrisMap[id.y - 1][id.x] == 1)
                    handleObj.GetComponent<BlockObjProperty>().canGoDown = false;
            }
            if (collision.transform.tag == "Ground")
            {
                handleObj.GetComponent<BlockObjProperty>().canGoDown = false;
            }
            if (collision.transform.tag == "LeftWall")
            {
                handleObj.GetComponent<BlockObjProperty>().canGoLeft = false;
            }
            if (collision.transform.tag == "RightWall")
            {
                handleObj.GetComponent<BlockObjProperty>().canGoRight = false;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (this.tag == "MovingBlock")
        {
            if (collision.transform.tag == "Ground")
            {
                handleObj.GetComponent<BlockObjProperty>().canGoDown = true;
            }
            if (collision.transform.tag == "LeftWall")
            {
                handleObj.GetComponent<BlockObjProperty>().canGoLeft = true;
            }
            if (collision.transform.tag == "RightWall")
            {
                handleObj.GetComponent<BlockObjProperty>().canGoRight = true;
            }
        }
    }
}
