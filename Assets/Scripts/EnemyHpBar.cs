using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHpBar : MonoBehaviour
{
    public Transform obj;
    public Slider hpBar;
    public GameObject player;

    Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;

    }

    // Update is called once per frame
    void Update()
    {
        if ((player.transform.position - obj.transform.position).magnitude > 10f)
            hpBar.enabled = false;
        else
        {
            hpBar.enabled = true;
            hpBar.transform.position = cam.WorldToScreenPoint(obj.position + new Vector3(0, 3f, 0));
        }
    }
}
