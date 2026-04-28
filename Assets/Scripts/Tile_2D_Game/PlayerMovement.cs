using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Stage stage;
    private Map map;
    private Animator animator;
    private int currentTileId;
    [SerializeField] private float moveInterval = 0.1f;
    private float moveTimer = 0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        GameObject findGo = GameObject.FindWithTag("Map");
        stage = findGo.GetComponent<Stage>();

    }

    private void Update()
    {
        if (stage.Map == null)
            return;

        moveTimer -= Time.deltaTime;

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

        if (direction != Sides.None && moveTimer <= 0f)
        {
            var targetTile = stage.Map.Tiles[currentTileId].Adjacents[(int)direction];
            if (targetTile != null && targetTile.CanMove)
            {
                MoveTo(targetTile.Id);
                moveTimer = moveInterval;
            }
        }
    }

    public void MoveTo(int tileId)
    {

        currentTileId = tileId;
        transform.position = stage.GetTilePos(currentTileId);
        stage.UpdateFog(currentTileId, 4);
    }

}
