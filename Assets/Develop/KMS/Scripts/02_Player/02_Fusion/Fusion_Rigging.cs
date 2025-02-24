using Fusion;
using UnityEngine;

public class Fusion_Rigging : NetworkBehaviour
{
    public Transform leftHandIK;            // 왼손 IK 
    public Transform rightHandIK;           // 오른손 IK
    public Transform headIK;                // HMD IK

    public Transform leftHandController;    // 왼손 컨트롤러
    public Transform rightHandController;   // 오른손 컨트롤러
    public Transform hmd;                   // HMD

    public Vector3[] leftOffset;            // 왼손 Offset
    public Vector3[] rightOffset;           // 오른손 Offset
    public Vector3[] headOffset;            // HMD Offset

    public float smoothValue = 0.1f;        // 부드럽게 움직일 값
    public float modelHeight = 1.1176f;     // 캐릭터 높이 값

    /// <summary>
    /// LateUpdate 대신 Fusion의 FixedUpdateNetwork를 사용하여 동기화
    /// </summary>
    public override void FixedUpdateNetwork()
    {
        if (Object.HasInputAuthority)
        {
            // 로컬 플레이어의 동작 처리
            MappingHandTransform(leftHandIK, leftHandController, true);
            MappingHandTransform(rightHandIK, rightHandController, false);
            MappingBodyTransform(headIK, hmd);
            MappingHeadTransform(headIK, hmd);

            // 동기화된 위치 및 회전을 RPC로 전송
            RPC_SyncIK(
                leftHandIK.position, leftHandIK.rotation,
                rightHandIK.position, rightHandIK.rotation,
                headIK.position, headIK.rotation);
        }
    }

    /// <summary>
    /// 컨트롤러의 싱크를 맞추기 위한 Offset.
    /// </summary>
    /// <param name="ik"></param>
    /// <param name="controller"></param>
    /// <param name="isLeft"></param>
    private void MappingHandTransform(Transform ik, Transform controller, bool isLeft)
    {
        // IK의 Transform = Controller의 Transform
        var offset = isLeft ? leftOffset : rightOffset;

        // 컨트롤러 위치 값
        ik.position = controller.TransformPoint(offset[0]);
        // 컨트롤러 회전 값
        ik.rotation = controller.rotation * Quaternion.Euler(offset[1]);
    }

    /// <summary>
    /// HMD와 캐릭터의 몸이 같이 돌아가도록 설정한 메서드.
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
    /// HMD의 IK Offset.
    /// </summary>
    /// <param name="ik"></param>
    /// <param name="hmd"></param>
    private void MappingHeadTransform(Transform ik, Transform hmd)
    {
        ik.position = hmd.TransformPoint(headOffset[0]);
        ik.rotation = hmd.rotation * Quaternion.Euler(headOffset[1]);
    }

    /// <summary>
    /// Fusion RPC로 동기화된 IK 데이터를 전송.
    /// </summary>
    /// <param name="leftHandPos"></param>
    /// <param name="leftHandRot"></param>
    /// <param name="rightHandPos"></param>
    /// <param name="rightHandRot"></param>
    /// <param name="headPos"></param>
    /// <param name="headRot"></param>
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RPC_SyncIK(Vector3 leftHandPos, Quaternion leftHandRot,
        Vector3 rightHandPos, Quaternion rightHandRot, Vector3 headPos, Quaternion headRot)
    {
        if (!Object.HasInputAuthority)
        {
            leftHandIK.position = leftHandPos;
            leftHandIK.rotation = leftHandRot;

            rightHandIK.position = rightHandPos;
            rightHandIK.rotation = rightHandRot;

            headIK.position = headPos;
            headIK.rotation = headRot;
        }
    }
}
