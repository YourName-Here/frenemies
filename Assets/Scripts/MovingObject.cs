﻿using UnityEngine;
using System.Collections;

public abstract class MovingObject : MonoBehaviour {
    public float TimeBetweenShots = 0.2f;
    public float MoveTime = 0.15f;
    public LayerMask BlockingLayer;

    private BoxCollider2D _boxcollider;
    private Rigidbody2D _rb2D;
    private float _inverseMoveTime;
    private bool _canShoot = true;

	protected virtual void Start () {
        _boxcollider = GetComponent<BoxCollider2D>();
        _rb2D = GetComponent<Rigidbody2D>();
        _inverseMoveTime = 1f / MoveTime;
	}

    protected bool Move(float xDir, float yDir) {
        Vector2 start = transform.position;
        Vector2 end = start + new Vector2(xDir, yDir);
        StartCoroutine(SmoothMovement(end));
        return true;
    }

    public void tryShoot(float xDir, float yDir) {
        if (_canShoot) {
            StartCoroutine(Shoot(xDir, yDir));
        }
    }

    private IEnumerator Shoot(float xDir, float yDir) {
        float offsetX = 0;
        float offSetY = 0;

        if (xDir > 0)
            offsetX = _boxcollider.size.x;
        else if (xDir < 0)
            offsetX = -1*_boxcollider.size.x;

        if (yDir > 0)
            offSetY = _boxcollider.size.y;
        else if (yDir < 0)
            offSetY = -1*_boxcollider.size.y;

        Vector3 direction = new Vector3(xDir, yDir, 0f);
        Vector3 startingPosition = transform.position + new Vector3(offsetX, offSetY, 0f);

        Object prefab = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Bullet.prefab", typeof (Bullet));
        Bullet clone = Instantiate(prefab, startingPosition, Quaternion.identity) as Bullet;
        clone.init(direction, this);

        _canShoot = false;
        yield return new WaitForSeconds(TimeBetweenShots);
        _canShoot = true;
    }

    protected virtual void AttemptMove<T>(float xDir, float yDir)
        where T : Component {
        bool canMove = Move(xDir, yDir);
    }

    protected IEnumerator SmoothMovement(Vector3 end) {
        Vector3 newPosition = Vector3.MoveTowards(_rb2D.position, end, _inverseMoveTime * Time.deltaTime);
        _rb2D.MovePosition(newPosition);
        yield return null;
    }

    protected abstract void OnCollisionEnter2D(Collision2D collision);
}
