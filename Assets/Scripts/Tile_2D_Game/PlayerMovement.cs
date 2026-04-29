using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public int CurrentTileId => currentTileId;

    private Stage stage;
    private Map map;
    private Animator animator;


    private int currentTileId = -1;
    private int targetTileId = -1;

    private bool isMoving = false;
    private Coroutine coMove = null;

    private Queue<int> pathQueue = new Queue<int>();

    private void Awake()
    {
        animator = GetComponent<Animator>();

        GameObject findGo = GameObject.FindWithTag("Map");
        stage = findGo.GetComponent<Stage>();

    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            int clickTileId = stage.ScreenPosToTileId(Input.mousePosition);

            if (clickTileId >= 0 && currentTileId >= 0)
            {
                Tile targetTile = stage.Map.Tiles[clickTileId];

                if (targetTile.CanMove)
                {
                    MoveTo(clickTileId);
                }
            }
        }

        if (stage.Map == null)
            return;

        var direction = Sides.None;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            direction = Sides.Top;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            direction = Sides.Bottom;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            direction = Sides.Right;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            direction = Sides.Left;
        }
        /*
        if (direction != Sides.None && !isMoving && currentTileId != -1)
        {
            var targetTile = stage.Map.Tiles[currentTileId].Adjacents[(int)direction];
            if (targetTile != null && targetTile.CanMove)
            {
                MoveTo(targetTile.Id);
            }
        }*/

        if (!isMoving && pathQueue.Count > 0)
        {
            int nextTileId = pathQueue.Dequeue();
            MoveTo(nextTileId);
        }

    }

    public void MoveTo(int tileId)
    {
        if (isMoving)
            return;

        targetTileId = tileId;

        if (coMove != null)
        {
            StopCoroutine(coMove);
            coMove = null;
        }

        coMove = StartCoroutine(CoMove());
    }

    public void Warp(int tileId)
    {
        if (coMove != null)
        {
            StopCoroutine(coMove);
            coMove = null;
        }
        isMoving = false;
        targetTileId = -1;
        currentTileId = tileId;
        transform.position = stage.GetTilePos(currentTileId);
        //stage.UpdateFog(currentTileId, 2);
    }

    public void SetPath(List<int> path)
    {
        pathQueue.Clear();

        for (int i = 1; i < path.Count; i++)
        {
            pathQueue.Enqueue(path[i]);
        }
    }

    public float moveSpeed = 50f;
    private IEnumerator CoMove()
    {

        isMoving = true;
        animator.speed = 1f;
        int currentTargetTileId = targetTileId;
        var path = stage.Map.PathFindingAstar(stage.Map.Tiles[currentTileId], stage.Map.Tiles[currentTargetTileId]);

        if (path.Count == 0)
        {
            isMoving = false;
            animator.speed = 0;
            coMove = null;
            yield break;
        }

        var pathIndex = 1;
        while (pathIndex < path.Count)
        {

            if (currentTargetTileId != targetTileId)
            {
                path = stage.Map.PathFindingAstar(stage.Map.Tiles[currentTileId], stage.Map.Tiles[currentTileId]);
                if (path.Count == 0)
                {
                    isMoving = false;
                    animator.speed = 0;
                    coMove = null;
                    yield break;
                }
                pathIndex = 1;
            }

            var startPos = transform.position;
            var endPos = stage.GetTilePos(targetTileId);
            var duration = Vector3.Distance(startPos, endPos) / moveSpeed;

            var t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            transform.position = endPos;

            currentTileId = targetTileId;
            targetTileId = -1;

            //stage.UpdateFog(currentTileId, 2);
        }

       
        isMoving = false;
        coMove = null;


        currentTileId = targetTileId;
        targetTileId = -1;

       // stage.UpdateFog(currentTileId, 2);
    }

}
