using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtr : MonoBehaviour
{
    public float rotationSpeed = 5.0f;
    private CinemachineFreeLook freeLookCamera;

    void Start()
    {
        // Cinemachine FreeLook ī�޶� ���� ��������
        freeLookCamera = GetComponent<CinemachineFreeLook>();
    }

    void Update()
    {
        // ���콺 �Է��� ����Ͽ� ī�޶� ȸ��
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // Cinemachine FreeLook ī�޶��� Orbit Input Axis�� ȸ�� ���� ����
        freeLookCamera.m_XAxis.m_InputAxisValue += mouseX * Time.deltaTime;
        freeLookCamera.m_YAxis.m_InputAxisValue += mouseY * Time.deltaTime;
    }
}
