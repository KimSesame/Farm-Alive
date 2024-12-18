using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class VR_Anim_Leg_Controller : MonoBehaviourPun
{
    public XRNode controllerNode = XRNode.LeftHand;     // �޼� ��Ʈ�ѷ�
    public float moveSpeed = 1.0f;                      // �̵� �ӵ�
    public Transform cameraTransform;                   // ī�޶� Transform
    public Animator animator;                           // Animator ������Ʈ
    private Vector2 inputAxis;                          // ���̽�ƽ �Է°�

    #region XR Origin�� ĳ���� �и���
    //private void Start()
    //{
    //    //if (!photonView.IsMine) return;

    //    animator = GetComponentInChildren<Animator>();
    //    if (animator == null)
    //        Debug.LogError("Animator�� ã�� �� �����ϴ�.");
    //    else
    //        Debug.Log("Animator ���� �Ϸ�");
    //}
    #endregion

    void Update()
    {
        if (photonView.IsMine)
        {    // ��Ʈ�ѷ� ���̽�ƽ �Է� ��������
            InputDevice controller = InputDevices.GetDeviceAtXRNode(controllerNode);
            if (controller.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputAxis))
            {
                Debug.Log("�Է� ����.");

                if (inputAxis != Vector2.zero)
                {
                    Debug.Log("�Է� �� ����.");

                    MoveCharacter();
                    animator.SetFloat("Speed", inputAxis.magnitude); // Speed �Ķ���� ����
                    Debug.Log("Speed: " + inputAxis.magnitude);
                }
                else
                {
                    animator.SetFloat("Speed", 0f); // ���� ����
                }

                photonView.RPC("SyncAnimationRPC", RpcTarget.Others, inputAxis.magnitude);
            }
        }

        #region test
        //if (photonView.IsMine)
        //{
        //    if (Input.GetKeyDown(KeyCode.C))
        //    {
        //        Debug.Log("�Է� �� ����.");

        //        MoveCharacter();
        //        animator.SetFloat("Speed", 1); // Speed �Ķ���� ����
        //        Debug.Log("CŰ ����!");
        //        photonView.RPC("SyncAnimationRPC", RpcTarget.Others, 1.0f);
        //    }
        //    else if (Input.GetKeyUp(KeyCode.C))
        //    {
        //        animator.SetFloat("Speed", 0f); // ���� ����
        //        photonView.RPC("SyncAnimationRPC", RpcTarget.Others, 0f);
        //    }
        //}
        #endregion

    }

    private void MoveCharacter()
    {
        if (photonView.IsMine)
        {
            // ī�޶� ������ �������� �̵� ���� ���
            Vector3 forward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;
            Vector3 right = new Vector3(cameraTransform.right.x, 0, cameraTransform.right.z).normalized;

            Vector3 moveDirection = (forward * inputAxis.y + right * inputAxis.x).normalized;

            // ĳ���� �̵�
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
    }

    [PunRPC]
    private void SyncAnimationRPC(float speed)
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", speed);
        }
    }
}