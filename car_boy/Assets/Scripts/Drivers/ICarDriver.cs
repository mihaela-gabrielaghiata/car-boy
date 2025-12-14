using UnityEngine;

public abstract class ICarDriver : MonoBehaviour
{
    public int FORWARD = 0;
    public int REVERSE = 1;
    public int LEFT = 2;
    public int RIGHT = 3;

    protected bool[] selectedDirection = { false, false, false, false }; // forward, reverse, left, right

    protected bool colided = false;

    public bool[] GetSelectedDirections()
    {
        return selectedDirection;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("TrackLimit"))
        {
            colided = true;
        }
    }

    public bool HasCollided()
    {
        return colided;
    }
}
