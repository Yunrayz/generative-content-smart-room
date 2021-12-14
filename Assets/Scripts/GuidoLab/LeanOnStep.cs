using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeanOnStep : MonoBehaviour
{
    GameObject player1;
    GameObject player2;
    public float minDistance = 1f;
    public float leanAngle = 50f;
    public bool inverted = false;
    private float normalizedDistance;
    // Start is called before the first frame update
    void Start()
    {
        player1 = GameObject.FindGameObjectsWithTag("Player")[0];
        player2 = GameObject.FindGameObjectsWithTag("Player")[1];

    }

    // Update is called once per frame
    void Update()
    {
        GameObject player;
        float distance, normalizedDistance;
        if (Vector3.Distance(transform.position, player1.transform.position) > Vector3.Distance(transform.position, player2.transform.position))
        {
            player = player2;
            distance = Vector3.Distance(transform.position, player2.transform.position);
            normalizedDistance = Mathf.InverseLerp(minDistance, 0, distance);
        }
        else
        {
            player = player1;
            distance = Vector3.Distance(transform.position, player1.transform.position);
            normalizedDistance = Mathf.InverseLerp(minDistance, 0, distance);
        }
        //if distance between player and object is less than 1.5f Lean opposite to the player
        // float distance = Vector3.Distance(transform.position, player.transform.position);
        // normalizedDistance = Mathf.InverseLerp(minDistance, 0, distance);
        if (distance < minDistance)
        {
            var leanRotation = Quaternion.Slerp(Quaternion.identity, Quaternion.Euler(inverted ? leanAngle : -leanAngle, transform.rotation.y, transform.rotation.z), normalizedDistance);
            // var lookRotation = Quaternion.Slerp(Quaternion.identity, Quaternion.LookRotation(player.transform.position - transform.position), 0.1f);
            // var leanRotation2 = Quaternion.Euler(Mathf.Lerp(90,-90,));
            var lookRotation = Quaternion.LookRotation(player.transform.position - transform.position);
            transform.rotation = Quaternion.Euler(leanRotation.eulerAngles.x, lookRotation.eulerAngles.y, transform.rotation.eulerAngles.z);

            // transform.rotation.LookRotation(player.transform);
            // transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(transform.rotation.x, Quaternion.LookRotation(player.transform.position - transform.position).y, transform.rotation.z), normalizedDistance);
            // transform.position = Vector3.Lerp(transform.position, GameObject.FindWithTag("Player").transform.position + new Vector3(0, 0, -1.5f), Time.deltaTime);
        }
    }

}
