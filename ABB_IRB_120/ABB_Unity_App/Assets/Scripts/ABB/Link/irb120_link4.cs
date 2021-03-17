// ------------------------------------------------------------------------------------------------------------------------//
// ----------------------------------------------------- LIBRARIES --------------------------------------------------------//
// ------------------------------------------------------------------------------------------------------------------------//

// -------------------- Unity -------------------- //
using UnityEngine;

public class irb120_link4 : MonoBehaviour
{
    void FixedUpdate()
    {
        transform.localEulerAngles = new Vector3(GlobalVariables_RWS_client.robotBaseRotLink_irb_joint[3], 0f, 0f);
    }
    void OnApplicationQuit()
    {
        Destroy(this);
    }
}
