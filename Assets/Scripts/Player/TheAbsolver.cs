using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheAbsolver : MonoBehaviour
{
    private LineRenderer lr;
    private Vector3 hookPoint;
    public LayerMask grappleable;
    public Transform muzzle, camTransform;
    public GameObject player;
    public float range = 150f;
    public SpringJoint spring;
    public bool grappling = false;

    [Header("Spring Settings")] public float damper, maxDistance, minDistance, springForce, massScale;
    
    void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(1) && !grappling)
        {
            grapple();
            grappling = true;           
        }
        else if(Input.GetMouseButtonUp(1) && grappling)
        {
            release();
            grappling = false;
            lr.positionCount = 0;
        }

        
    }

    private void LateUpdate()
    {
        if (grappling)
        {
            lr.SetPosition(0, muzzle.position);
            lr.SetPosition(1,hookPoint);
        }
        
    }

    public void grapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(camTransform.position, camTransform.forward, out hit, range, grappleable))
        {
            lr.positionCount = 2;
            hookPoint = hit.point;
            spring = player.AddComponent<SpringJoint>();
            spring.autoConfigureConnectedAnchor = false;
            spring.connectedAnchor = hookPoint;
            

            float distance = Vector3.Distance(hookPoint, player.transform.position);
            
            spring.damper = damper;
            spring.maxDistance = distance * maxDistance;
            spring.minDistance = distance *minDistance;
            spring.spring = springForce;
            spring.massScale = massScale;
        }
    }

    public void release()
    {
        Destroy(spring);
    }
}
