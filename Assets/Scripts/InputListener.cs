using UnityEngine;

namespace MyTapMatch
{
    public class InputListener : MonoBehaviour
    {
        [SerializeField] GameClient _client;

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                var hit = Physics2D.Raycast(ray.origin, ray.direction);

                if (hit.collider != null)
                {
                    var view = hit.collider.GetComponent<PlayableView>();
                    if (view != null)
                    {
                        _client.ClickOn(view);
                    }
                }
            }
        }
    }
}


