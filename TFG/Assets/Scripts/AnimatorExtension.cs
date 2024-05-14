using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatorExtension : MonoBehaviour
{
    Animator animator_;
    // Start is called before the first frame update
    void Awake()
    {
        animator_ = GetComponent<Animator>();
    }

    public void SetBoolTrue(string name) => animator_.SetBool(name, true);

    public void SetBoolFalse(string name) => animator_.SetBool(name, false);

}
