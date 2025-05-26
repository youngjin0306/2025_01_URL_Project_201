using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class MeshTrail : MonoBehaviour
{
    public float activeTime = 2.0f;   //잔상 효과 지속 시간
    public MovementInput moveScript;  //캐릭터의 움직임을 제어 하는 스크립트
    public float speedBoost = 6;      //잔향 효과 사용시 속도 증가량
    public Animator animator;         //캐릭터의 애니메이션을 제어 하는 컴포넌트
    public float animSpeedBoost = 1.5f; //잔상 효과 사용시 애니메이션 속도 증가량

    [Header(" Mesh Releted")]
    public float meshRefreshRate = 1.0f;
    public float meshDestoryDelay = 3.0f;
    public Transform positionToSpawn;

    [Header("Shader Releted")]
    public Material mat;
    public string shaderVerRef;
    public float shaderVarRate = 0.1f;
    public float shaderVarRefreshRate = 0.05f;

    private SkinnedMeshRenderer[] skinnedRenderer;
    private bool isTrailActive;

    private float normalSpeed;
    private float normalAnimSpeed;

    //쟤질의 투명도를 서서히 변경하는 코투틴
    IEnumerator AnimateMatralFloat(Material m, float valueGoal, float rate, float refreshRate)
    {
        float valueToAnimate = m.GetFloat(shaderVerRef); //알파 값을 가져온다.

        //목표 값에 도달 할 때 까지 반복
        while (valueToAnimate > valueGoal)
        {
            valueToAnimate -= rate;
            m.SetFloat(shaderVerRef, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }

    IEnumerator ActivateTrail(float timeActivated)        //잔상 효과 발동
    {
        normalSpeed = moveScript.movementSpeed;        //현재 속도를 저장하고 증가된 속도 적용
        moveScript.movementSpeed = speedBoost;

        normalAnimSpeed = animator.GetFloat("animSpeed");  //현재 애니메이션 속도를 저장하고 증가된 속도 적용
        animator.SetFloat("animSpeed", animSpeedBoost);

        while(timeActivated > 0)
        {
            timeActivated -= meshRefreshRate;            //시간 카운트를 한다. 0쪽으로

            if (skinnedRenderer == null)
                skinnedRenderer = positionToSpawn.GetComponentsInChildren<SkinnedMeshRenderer>();   //생성된 위치의 렌더러 컴포넌트들을 가져옴

            for(int i = 0; i < skinnedRenderer.Length; i++)      //각 메시 렌더러의 대한 잔상 생성
            {
                GameObject g0bj = new GameObject();      //새로운 오브젝트 생성
                g0bj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer mr = g0bj.AddComponent<MeshRenderer>();
                MeshFilter mf = g0bj.AddComponent<MeshFilter>();

                Mesh m = new Mesh();                    //현재 캐릭터의 포즈를 메시로 변환
                skinnedRenderer[i].BakeMesh(m);
                mf.mesh = m;
                mr.material = mat;
                //잔상의 페이드 아웃 효과 시작
                StartCoroutine(AnimateMatralFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));

                Destroy(g0bj, meshDestoryDelay);
            }
                //다음 잔상 생성까지 대기
            yield return new WaitForSeconds(meshRefreshRate);     //일정 시간 후 잔상 제거
            

            //원래 속도와 애니메이션 속도로 복구
            moveScript.movementSpeed = normalSpeed;
            animator.SetFloat("animSpeed", normalAnimSpeed);
            isTrailActive = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTrailActive)    //스페이스바를 누르고 현재 잔상 효과가 비활성화일 때
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(activeTime));        //잔상 효과 코루틴 시작
        }
    }
}
