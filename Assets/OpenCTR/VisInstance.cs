using System;
using System.Collections;
using System.Collections.Generic;
using CTRFramework;
using OpenCTR.Level;
using UnityEditor;
using UnityEngine;

public class VisInstance : MonoBehaviour
{
    public VisData Visi;
    [SerializeField] private int leftChild;
    [SerializeField] private int rightChild;
    [SerializeField] private uint numQuadBlock;
    [SerializeField] private uint ptrQuadBlock;
    [SerializeField] private ushort u0;
    [SerializeField] private uint u1;
    [SerializeField] private uint ptrUnkData;
    [SerializeField] private byte flag;
    [SerializeField] private VisDataFlags visflag;
    [SerializeField] private ushort id;
    private LevelHandler levelHandler;
    public VisInstance()
    {
    }
    public VisInstance(VisData visi)
    {
        Visi = visi;
        leftChild = visi.leftChild;
        rightChild = visi.rightChild;
        u0 = visi.unk;
        u1 = visi.unk1;
        ptrUnkData = visi.ptrUnkData;
        numQuadBlock = Visi.numQuadBlock;
        ptrQuadBlock = Visi.ptrQuadBlock;
    }
    public void Set(VisData visi)
    {
        Visi = visi;
        leftChild = visi.leftChild;
        rightChild = visi.rightChild;
        u0 = visi.unk;
        u1 = visi.unk1;
        ptrUnkData = visi.ptrUnkData;
        numQuadBlock = Visi.numQuadBlock;
        ptrQuadBlock = Visi.ptrQuadBlock;
        flag = Visi.byteflag;
        visflag = Visi.flag;
        id = visi.id;
        foreach (var qb in visi.QuadPointers)
        {
            qb.view.VisPointer = this;
            qb.view.flag = Visi.byteflag;
            qb.view.visflag = Visi.flag;
            qb.view.unk0vis = Visi.unk0;
            qb.view.unk1vis = Visi.unk1;
        }
    }

    public void SetSceneHandler(LevelHandler sh)
    {
        levelHandler = sh;
    }
    private void OnDrawGizmosSelected()
    {
        if (Visi == null) return;
        if (!Visi.IsLeaf)
        {
            leftChild = Visi.leftChild;
            rightChild = Visi.rightChild;
            if (Selection.Contains(transform.gameObject)) {
                Gizmos.color = new Color(1, 1, 1, 0.5f);
            }
            else
            {
                Gizmos.color = new Color(1, 1, 1, 0.1f);
            }
            Vector3 bmax = new Vector3(Visi.bbox.Max.X / 100.0f, Visi.bbox.Max.Y / 100.0f, -Visi.bbox.Max.Z / 100.0f);
            Vector3 bmin = new Vector3(Visi.bbox.Min.X / 100.0f, Visi.bbox.Min.Y / 100.0f, -Visi.bbox.Min.Z / 100.0f);
            Vector3 bsize = new Vector3(Mathf.Abs(bmax.x - bmin.x), Mathf.Abs(bmax.y - bmin.y), Mathf.Abs(bmax.z - bmin.z));
            Vector3 bpos = new Vector3((bmax.x + bmin.x) / 2.0f, (bmax.y + bmin.y) / 2.0f,
                (bmax.z + bmin.z) / 2.0f);
            transform.position = levelHandler.transform.position + bpos;

            Gizmos.DrawWireCube(transform.position, bsize);
        }
        else
        {
            
            numQuadBlock = Visi.numQuadBlock;
            ptrQuadBlock = Visi.ptrQuadBlock;
            Gizmos.color = new Color(0, 0, 1, 0.5f);
            Gizmos.DrawWireCube(transform.parent.position, new Vector3(1, 1, 1));
            Gizmos.color = new Color(1, 0, 0, 0.333f);
            //if (Selection.Contains(transform.gameObject)) 
            {
                Gizmos.color = new Color(1, 1, 0, 0.333f);
                //ptrQuadBlock = ptrQuadBlock >> 16;
                ptrQuadBlock = (uint) (((ptrQuadBlock) / levelHandler.GetLevelShiftDivide())  + levelHandler.GetLevelShiftOffset());
                QuadBlock[] quads = levelHandler.quads.ToArray();
                for (int i = 0; i < numQuadBlock; i++)
                {
                    long pointer = ptrQuadBlock + i;
                    if (pointer < 0 || pointer > quads.Length - 1) continue;
                    QuadBlock quad = quads[pointer];
                    Vertex[] vertList = quad.GetVertexList(levelHandler.verts).ToArray();
                    for (int j = 0; j < vertList.Length-1; j++)
                    {
                        Vertex vertA = vertList[j];
                        Vertex vertB = vertList[j+1];
                        Vector3 a = Vector3.Scale(vertA.Position, new Vector3(1f,1f,-1f));
                        Vector3 b = Vector3.Scale(vertB.Position, new Vector3(1f,1f,-1f));
                        var position = levelHandler.transform.position;
                        a += position;
                        b += position;
                        Gizmos.DrawLine(a,b);
                    }
                }
            }
        }
    }
}