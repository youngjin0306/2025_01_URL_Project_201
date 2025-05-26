using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class MeshTrail : MonoBehaviour
{
    public float activeTime = 2.0f;   //�ܻ� ȿ�� ���� �ð�
    public MovementInput moveScript;  //ĳ������ �������� ���� �ϴ� ��ũ��Ʈ
    public float speedBoost = 6;      //���� ȿ�� ���� �ӵ� ������
    public Animator animator;         //ĳ������ �ִϸ��̼��� ���� �ϴ� ������Ʈ
    public float animSpeedBoost = 1.5f; //�ܻ� ȿ�� ���� �ִϸ��̼� �ӵ� ������

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

    //������ ������ ������ �����ϴ� ����ƾ
    IEnumerator AnimateMatralFloat(Material m, float valueGoal, float rate, float refreshRate)
    {
        float valueToAnimate = m.GetFloat(shaderVerRef); //���� ���� �����´�.

        //��ǥ ���� ���� �� �� ���� �ݺ�
        while (valueToAnimate > valueGoal)
        {
            valueToAnimate -= rate;
            m.SetFloat(shaderVerRef, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }

    IEnumerator ActivateTrail(float timeActivated)        //�ܻ� ȿ�� �ߵ�
    {
        normalSpeed = moveScript.movementSpeed;        //���� �ӵ��� �����ϰ� ������ �ӵ� ����
        moveScript.movementSpeed = speedBoost;

        normalAnimSpeed = animator.GetFloat("animSpeed");  //���� �ִϸ��̼� �ӵ��� �����ϰ� ������ �ӵ� ����
        animator.SetFloat("animSpeed", animSpeedBoost);

        while(timeActivated > 0)
        {
            timeActivated -= meshRefreshRate;            //�ð� ī��Ʈ�� �Ѵ�. 0������

            if (skinnedRenderer == null)
                skinnedRenderer = positionToSpawn.GetComponentsInChildren<SkinnedMeshRenderer>();   //������ ��ġ�� ������ ������Ʈ���� ������

            for(int i = 0; i < skinnedRenderer.Length; i++)      //�� �޽� �������� ���� �ܻ� ����
            {
                GameObject g0bj = new GameObject();      //���ο� ������Ʈ ����
                g0bj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer mr = g0bj.AddComponent<MeshRenderer>();
                MeshFilter mf = g0bj.AddComponent<MeshFilter>();

                Mesh m = new Mesh();                    //���� ĳ������ ��� �޽÷� ��ȯ
                skinnedRenderer[i].BakeMesh(m);
                mf.mesh = m;
                mr.material = mat;
                //�ܻ��� ���̵� �ƿ� ȿ�� ����
                StartCoroutine(AnimateMatralFloat(mr.material, 0, shaderVarRate, shaderVarRefreshRate));

                Destroy(g0bj, meshDestoryDelay);
            }
                //���� �ܻ� �������� ���
            yield return new WaitForSeconds(meshRefreshRate);     //���� �ð� �� �ܻ� ����
            

            //���� �ӵ��� �ִϸ��̼� �ӵ��� ����
            moveScript.movementSpeed = normalSpeed;
            animator.SetFloat("animSpeed", normalAnimSpeed);
            isTrailActive = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTrailActive)    //�����̽��ٸ� ������ ���� �ܻ� ȿ���� ��Ȱ��ȭ�� ��
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(activeTime));        //�ܻ� ȿ�� �ڷ�ƾ ����
        }
    }
}
