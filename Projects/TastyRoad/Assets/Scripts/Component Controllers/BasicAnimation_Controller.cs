using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAnimation_Controller : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    public SpriteRenderer spriteRenderer => _spriteRenderer;

    private Animator _animator;

    [SerializeField] private string _defaultAnimation;



    // UnityEngine
    private void Awake()
    {
        if (gameObject.TryGetComponent(out SpriteRenderer spriteRenderer)) { _spriteRenderer = spriteRenderer; }
        if (gameObject.TryGetComponent(out Animator animator)) { _animator = animator; }
    }

    private void Start()
    {
        Set_DefaultAnimation();
    }



    // Custon Animation Play
    private void Set_DefaultAnimation()
    {
        if (_defaultAnimation == null) return;

        _animator.Play(_defaultAnimation);
    }

    public void Play_Animation(string animationName)
    {
        _animator.Play(animationName);
    }



    // Sprite Flip Control
    public void Flip_Sprite(bool facingLeft)
    {
        _spriteRenderer.flipX = facingLeft;
    }
    public void Flip_Sprite(float faceDirection)
    {
        if (faceDirection < 0)
        {
            Flip_Sprite(true);
        }
        else if (faceDirection > 0)
        {
            Flip_Sprite(false);
        }
    }
    public void Flip_Sprite(GameObject targetObject)
    {
        if (transform.position.x > targetObject.transform.position.x)
        {
            Flip_Sprite(true);
        }
        else if (transform.position.x < targetObject.transform.position.x)
        {
            Flip_Sprite(false);
        }
    }



    // Basic Animation Control
    public void Idle_Move(bool isMoving)
    {
        _animator.SetBool("isMoving", isMoving);
    }
}