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
        // Cinemachine FreeLook 카메라 참조 가져오기
        freeLookCamera = GetComponent<CinemachineFreeLook>();
    }

    void Update()
    {
        // 마우스 입력을 사용하여 카메라 회전
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // Cinemachine FreeLook 카메라의 Orbit Input Axis에 회전 값을 적용
        freeLookCamera.m_XAxis.m_InputAxisValue += mouseX * Time.deltaTime;
        freeLookCamera.m_YAxis.m_InputAxisValue += mouseY * Time.deltaTime;
    }
}
