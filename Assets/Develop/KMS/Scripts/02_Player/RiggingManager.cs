using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiggingManager : MonoBehaviourPun
{
    public Transform leftHandIK;            // �޼� IK 
    public Transform righttHandIK;          // ������ IK
    public Transform headIK;                // HMD IK

    public Transform leftHandController;    // �޼� ��Ʈ�ѷ�
    public Transform rightHandController;   // ������ ��Ʈ�ѷ�
    public Transform hmd;                   // HMD

    public Vector3[] leftOffset;            // �޼� Offset
    public Vector3[] rightOffset;           // ������ Offset
    public Vector3[] headOffset;            // hmd Offset

    public float smoothValue = 0.1f;        // �ε巴�� ������ ��
    public float modelHeight = 1.1176f;     // ĳ���� ���� ��


    #region XR Origin�� ĳ���� �и���
    //private void Start()
    //{
    //// PhotonView.IsMine�� ��쿡�� ����
    //if (photonView.IsMine)
    //{
    //    FindControllers();
    //}
    //}
    #endregion

    /// <summary>
    /// ��Ʈ�ѷ��� ������ �� IK�� Transform�� ���߷���.
    /// </summary>
    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            // ���� �÷��̾��� ���� ó��
            MappingHandTranform(leftHandIK, leftHandController, true);
            MappingHandTranform(righttHandIK, rightHandController, false);
            MappingBodyTransform(headIK, hmd);
            MappingHeadTransform(headIK, hmd);

            // ����ȭ�� ��ġ �� ȸ���� RPC�� ����
            photonView.RPC("SyncIKRPC", RpcTarget.Others, 
                leftHandIK.position, leftHandIK.rotation,
                righttHandIK.position, righttHandIK.rotation, 
                headIK.position, headIK.rotation);
        }
    }

    #region XR Origin�� ĳ���� �и���
    //private void FindControllers()
    //{
    //    // XR Origin�� ã�Ƽ� ��Ʈ�ѷ��� HMD ����
    //    GameObject xrOrigin = GameObject.Find("Player(XR Origin)(Clone)");

    //    if (xrOrigin != null)
    //    {
    //        leftHandController = xrOrigin.transform.Find("Camera Offset/Left Controller");
    //        rightHandController = xrOrigin.transform.Find("Camera Offset/Right Controller");
    //        hmd = xrOrigin.transform.Find("Camera Offset/Main Camera");

    //        Debug.Log("RiggingManager�� ��Ʈ�ѷ� ����Ϸ�");
    //    }
    //    else
    //    {
    //        Debug.LogError("Player(XR Origin) ã������! XR Origin�̸� Ʋ��.");
    //    }
    //}
    #endregion

    /// <summary>
    /// ��Ʈ�ѷ��� ��ũ�� ���߱� ���� Offset.
    /// </summary>
    /// <param name="ik"></param>
    /// <param name="controller"></param>
    /// <param name="isLeft"></param>
    private void MappingHandTranform(Transform ik, Transform controller, bool isLeft)
    {
        // ik�� Transform = Controller�� Transform
        var offset = isLeft ? leftOffset : rightOffset;

        // ��Ʈ�ѷ� ��ġ ��. [0]
        ik.position = controller.TransformPoint(offset[0]);
        // ��Ʈ�ѷ� ȸ�� ��. [1]
        ik.rotation = controller.rotation * Quaternion.Euler(offset[1]);
    }

    /// <summary>
    /// HMD�� ĳ������ ���� ���� ���ư����� ������ �޼���.
    /// </summary>
    /// <param name="ik"></param>
    /// <param name="hmd"></param>
    private void MappingBodyTransform(Transform ik, Transform hmd)
    {
        this.transform.position = new Vector3(hmd.position.x, hmd.position.y - modelHeight, hmd.position.z);
        float yaw = hmd.eulerAngles.y;
        var targetRotation = new Vector3(this.transform.eulerAngles.x, yaw, this.transform.eulerAngles.z);
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.Euler(targetRotation), smoothValue);
    }

    /// <summary>
    /// HMD�� IK Offset.
    /// </summary>
    /// <param name="ik"></param>
    /// <param name="hmd"></param>
    private void MappingHeadTransform(Transform ik, Transform hmd)
    {
        ik.position = hmd.TransformPoint(headOffset[0]);
        ik.rotation = hmd.rotation * Quaternion.Euler(headOffset[1]);
    }

    /// <summary>
    /// IK�� ����ȭ �� RPC.
    /// </summary>
    /// <param name="leftHandPos"></param>
    /// <param name="leftHandRot"></param>
    /// <param name="rightHandPos"></param>
    /// <param name="rightHandRot"></param>
    /// <param name="headPos"></param>
    /// <param name="headRot"></param>
    [PunRPC]
    private void SyncIKRPC(Vector3 leftHandPos, Quaternion leftHandRot,
        Vector3 rightHandPos, Quaternion rightHandRot, Vector3 headPos, Quaternion headRot)
    {
        leftHandIK.position = leftHandPos;
        leftHandIK.rotation = leftHandRot;

        righttHandIK.position = rightHandPos;
        righttHandIK.rotation = rightHandRot;

        headIK.position = headPos;
        headIK.rotation = headRot;
    }
}