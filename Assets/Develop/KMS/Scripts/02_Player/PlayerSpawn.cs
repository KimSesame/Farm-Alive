using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class PlayerSpawn : MonoBehaviourPun
{
    [SerializeField] private Camera cam;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private TrackedPoseDriver trackedPoseDriver;

    //[SerializeField] private ActionBasedControllerManager leftControllerManager;
    //[SerializeField] private ActionBasedControllerManager rightControllerManager;
    [SerializeField] private ActionBasedController leftController;
    [SerializeField] private ActionBasedController rightController;

    private void Awake()
    {
        Debug.Log($"PhotonView Owner: {photonView.Owner}, IsMine: {photonView.IsMine}");

        if (!photonView.IsMine)
        {
            // TODO : �� ĳ���Ͱ� �ƴ� VR�÷��̾��
            // 1. ī�޶� ��Ȱ��ȭ ��Ų��.
            cam.enabled = false;
            // 2. ����� �����ʸ� ��Ȱ��ȭ ��Ų��.
            audioListener.enabled = false;
            // 3. TRacked Pose Driver�� ��Ȱ��ȭ �Ͽ�, �Է¿� ���� ī�޶� �������� �ʵ��� �Ѵ�.
            trackedPoseDriver.enabled = false;
            // 4. ��Ʈ�ѷ��� �Է��� ��Ȱ��ȭ ��Ų��.
            leftController.enabled = false;
            rightController.enabled = false;

            //leftControllerManager.enabled = false;
            //rightControllerManager.enabled = false;
        }
    }
}