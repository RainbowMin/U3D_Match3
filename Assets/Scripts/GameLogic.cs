using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameLogic : MonoBehaviour 
{
    public int SPRITE_GAP = 10;
    static public int SPRITE_COUNT = 6;
    public GameObject[] SpritePrefabList = new GameObject[SPRITE_COUNT];
    static public int m_MatrixCol = 5;
    static public int m_MatrixRow = 8;
    public float DropVelocity = 2.0f;
    public float DropHeight = 4.0f;
    public GameObject m_ParticleEffect;
    public GameObject m_SelectSprite;

    public int m_LeftBottomX;
    public int m_LeftBottomY;
    private int m_SpriteWidth;
    private int m_SpriteHeight;
    private GameObject[,] m_SpriteArray = new GameObject[m_MatrixRow, m_MatrixCol];//8行5列
    private bool m_IsAnimating = true;
    private GameObject m_SrcSprite = null;
    private GameObject m_DestSprite = null;
    

	void Start () 
    {
        SpriteRenderer renderer = SpritePrefabList[0].GetComponent<SpriteRenderer>();
        m_SpriteWidth = (int)renderer.sprite.rect.width;
        m_SpriteHeight = (int)renderer.sprite.rect.height;

		//Unity的坐标原点是场景的中心
        //计算得到寿司矩阵的左下角起始点
        m_LeftBottomX = -1 * (m_SpriteWidth * m_MatrixCol + (m_MatrixCol - 1) * SPRITE_GAP) / 2;
        m_LeftBottomY = -1 * (m_SpriteHeight * m_MatrixRow + (m_MatrixRow - 1) * SPRITE_GAP) / 2;

        InitAllSprite();
        m_SelectSprite.active = false;
    }

	IEnumerator  WaitSeconds(float time)
	{
		yield return new WaitForSeconds(time);

		CheckAndRemoveSprite();
	}

    void Update()
    {
        /*寿司精灵下落后，同行或同列如果有三个或三个以上相同的寿司精灵，那么应该首先判断并将其消除。
        而按照常理，在寿司下落过程中是不能进行消除操作的，所以我们要先定义一个变量，
        判断是否有精灵处于下落状态（即runAction是否完成）。如全部都落到了指定位置，则开始检测是否有满足消除条件的寿司，
        如有，则将其消除并“播放”爆炸特效，同时新建寿司精灵填补空位。*/
        
        if(m_IsAnimating)
        {
            m_IsAnimating = !IsAllSpriteAnimationEnd();
        }

        if(!m_IsAnimating)
        {            
            CheckAndRemoveSprite();
        }

        if(!m_IsAnimating)
        {
            CheckIsHit();
        }
    }

    void InitAllSprite()
    {

        for (int row = 0; row < m_MatrixRow; row++)
        {
            for (int col = 0; col < m_MatrixCol; col++)
            {
				CreateSpriteAndDrop(row,col);
            }
        }
    }

	void CreateSpriteAndDrop(int row, int col)
	{
		//创建并执行下落动画
		int SpriteIndex =  Random.Range(0, SPRITE_COUNT);

        //for test 固定下落的组合
        //if(col == 0)
        //{
        //    if (row == 0 || row == 3 || row == 5 || row == 7)
        //    {
        //        SpriteIndex = 1;
        //    }
        //    else
        //    {
        //        SpriteIndex = 0;
        //    }
        //}

		GameObject sprite = (GameObject)GameObject.Instantiate(SpritePrefabList[SpriteIndex]);
		m_SpriteArray[row, col] = sprite;

		Vector3 EndPos = GetSpritePosition(row, col);
		//Vector3 StartPos = new Vector3(EndPos.x, EndPos.y + (float)(Screen.height / 2));
        Vector3 StartPos = new Vector3(EndPos.x, EndPos.y + DropHeight);
        float DropTime = (StartPos.y - EndPos.y) / DropVelocity;
        DropSprite(sprite, StartPos, EndPos, DropTime);	
	}

	void  DropSprite(GameObject sprite, Vector3 StartPos, Vector3 EndPos, float time)
	{
        m_IsAnimating = true;
        //iTween的onupdate不会马上触发，会导致会错误的判断成动画已结束，所以这里人为地设置为正在动画状态
        sprite.SendMessage("SetIsAnimating", SendMessageOptions.DontRequireReceiver);

		sprite.transform.position = StartPos;
		iTween.MoveTo(sprite, iTween.Hash("time", time, "position", EndPos, "easetype", iTween.EaseType.bounce, "oncomplete", "AnimationEnd", "oncompletetarget", sprite, "onupdate", "AnimationUpdate", "onupdatetarget", sprite));
	}

    //得到对应行列精灵的坐标值, 得到的坐标是精灵的中央
    Vector3 GetSpritePosition(int row, int col)
    {
        int x = m_LeftBottomX + (m_SpriteWidth + SPRITE_GAP) * col + m_SpriteWidth / 2;
        int y = m_LeftBottomY + (m_SpriteHeight + SPRITE_GAP) * row + m_SpriteHeight / 2;

        float fX = (float)x / 100.0f;
        float fY = (float)y / 100.0f;

        return new Vector3(fX, fY, 0);
    }

	void GetSpriteInfo(Vector3 pos, out int row, out int col)
	{
		int x = (int)(pos.x * 100);
		int y = (int)(pos.y * 100);

		col = (x - m_LeftBottomX - m_SpriteWidth / 2) / (m_SpriteWidth + SPRITE_GAP);
		row = (y - m_LeftBottomY - m_SpriteHeight / 2) / (m_SpriteHeight + SPRITE_GAP);
	}

    bool IsAllSpriteAnimationEnd()
    {
        GameObject[] SpriteList = GameObject.FindGameObjectsWithTag("Sushi");
        foreach (GameObject o in SpriteList)
        {
            SushiSprite sprite = o.GetComponent<SushiSprite>();
            if(sprite != null)
            {
                if (!sprite.IsAnmationEnd())
                {
                    return false;
                }
            }
            else
            {
                Debug.LogError("error");
            }
        }

        return true;
    }

    void CheckAndRemoveSprite()
    {
        for (int row = 0; row < m_MatrixRow; row++)
        {
            for (int col = 0; col < m_MatrixCol; col++)
            {
                if (m_SpriteArray[row, col] == null)
                {
                    continue;
                }

                List<GameObject> RowChainList = new List<GameObject>();
                GetRowChain(row, col, RowChainList);

                List<GameObject> ColChainList = new List<GameObject>();
                GetColChain(row, col, ColChainList);

                List<GameObject> LongerChainList = RowChainList.Count > ColChainList.Count ? RowChainList : ColChainList;
                if (LongerChainList.Count == 3)
                {
					RemoveSprites(LongerChainList);
                    return;
                }
                else if (LongerChainList.Count > 3)
                {
                    //todo 4连以上有特殊处理
					RemoveSprites(LongerChainList);
                    return;
                }
            }
        }
    }

    void GetRowChain(int row, int col, List<GameObject> ChainList)
    {
        GameObject sprite = m_SpriteArray[row, col];
        ChainList.Add(sprite);

        int neighborCol = col - 1;
        while (neighborCol >= 0)
        {
            GameObject o = m_SpriteArray[row, neighborCol];
            if (o != null && o.name == sprite.name)
            {
                ChainList.Add(o);
                neighborCol--;
            }
            else
            {
                break;
            }
        }

        neighborCol = col + 1;
        while (neighborCol < m_MatrixCol)
        {
            GameObject o = m_SpriteArray[row, neighborCol];
            if (o != null && o.name == sprite.name)
            {
                ChainList.Add(o);
                neighborCol++;
            }
            else
            {
                break;
            }
        }
    }

    void GetColChain(int row, int col, List<GameObject> ChainList)
    {
        GameObject sprite = m_SpriteArray[row, col];
        ChainList.Add(sprite);

        int neighborRow = row - 1;
        while (neighborRow >= 0)
        {
            GameObject o = m_SpriteArray[neighborRow, col];
            if (o != null && o.name == sprite.name)
            {
                ChainList.Add(o);
                neighborRow--;
            }
            else
            {
                break;
            }
        }

        neighborRow = row + 1;
        while (neighborRow < m_MatrixRow)
        {
            GameObject o = m_SpriteArray[neighborRow, col];
            if (o != null && o.name == sprite.name)
            {
                ChainList.Add(o);
                neighborRow++;
            }
            else
            {
                break;
            }
        }
    }

    void PlayEffect(Vector3 pos)
    {
        if (m_ParticleEffect == null) return;
        GameObject obj = (GameObject)GameObject.Instantiate(m_ParticleEffect);
        pos.z = -6.0f;
        //obj.transform.position = pos;
        obj.transform.localPosition = pos;
    }

    void RemoveSprites(List<GameObject> SpriteList)
    {
        foreach (GameObject o in SpriteList)
        {
			if(o != null)
			{
                int row, col;
                GetSpriteInfo(o.transform.position, out row, out col);
                m_SpriteArray[row, col] = null;

                //播放特效
                PlayEffect(o.transform.position);

                //Debug.Log("RemoveSprites, row=" + row + " col=" + col);

				GameObject.Destroy(o);
			}
		}
        
        FillEmpty();
    }

    void FillEmpty()
    {
        //填补空缺的寿司矩阵位置可以分为两个过程：
        //1、让空缺处上面的寿司精灵依次“落”到空缺处 
        //2、创建新的寿司精灵并让它“落”到矩阵最上方空缺的位置。

        int[] EmptyCountOfColList = new int[m_MatrixCol];

        //1. 让空缺处上面的精灵向下落
		int EmptySpriteCountOfCol;
        for (int col = 0; col < m_MatrixCol; col++)
        {
            EmptySpriteCountOfCol = 0;//记录一列中空缺的精灵数

            for (int row = 0; row < m_MatrixRow; row++)
            {
                GameObject sprite = m_SpriteArray[row, col];
                if(sprite == null)
                {
                    EmptySpriteCountOfCol++;
					//Debug.Log("row="+row+" col="+col+" sprite=null");
                }
				else
				{
					if(EmptySpriteCountOfCol > 0)
					{
						//计算寿司精灵的新行数
						int newRow = row - EmptySpriteCountOfCol;

						//Debug.Log("DropSprite row="+row+" col="+col+" new row="+newRow+" Empty="+EmptySpriteCountOfCol);

						m_SpriteArray[row,col] = null;
						m_SpriteArray[newRow,col] = sprite;

						Vector3 StartPos = GetSpritePosition(row,col);
						Vector3 EndPos = GetSpritePosition(newRow, col);
                        float dropTime = (StartPos.y - EndPos.y) / DropVelocity;
                        DropSprite(sprite, StartPos, EndPos, dropTime);
					}
				}
            }

            EmptyCountOfColList[col] = EmptySpriteCountOfCol;
        }

		//2.创建新的寿司精灵并让它“落”到矩阵最上方空缺的位置
        for (int col = 0; col < m_MatrixCol; col++)
        {
            for (int row = m_MatrixRow - EmptyCountOfColList[col]; row < m_MatrixRow; row++)
            {
                CreateSpriteAndDrop(row, col);
            }
        }
    }

    void CheckIsHit()
    {
        //手势划过两个区域来交换，此时只有一次手按下抬起操作
        //手点击一下选中Src, 再点击一下选中Dest, 此时有两次手按下抬起操作

        int LeftMouseBtn = 0;
        if (Input.GetMouseButtonDown(LeftMouseBtn))
        {            
            GetSrcSprite();
            if (m_SrcSprite)
            {
                m_SelectSprite.active = true;
                m_SelectSprite.transform.position = m_SrcSprite.transform.position;
            }
            //Debug.Log("GetSrcSprite();");
        }

        if (Input.GetMouseButtonUp(LeftMouseBtn))
        {
            GetDestSprite();
            //Debug.Log("GetDestSprite();");
        }

        if (m_DestSprite != null && m_SrcSprite != null && m_DestSprite != m_SrcSprite)
        {
            //Debug.Log("SwapSprite();");

            //交换
            SwapSprite(m_SrcSprite, m_DestSprite);
            m_SrcSprite = null;
            m_DestSprite = null;
            m_SelectSprite.active = false;
        }
    }

    private void GetSrcSprite()
    {
        Vector3 v3_Screen = Input.mousePosition;
        Vector3 v3_World = Camera.main.ScreenToWorldPoint(v3_Screen);
        m_SrcSprite = GetSpriteOfTouchPoint(new Vector3(v3_World.x, v3_World.y, 0));
    }

    private void GetDestSprite()
    {
        Vector3 v3_Screen = Input.mousePosition;
        Vector3 v3_World = Camera.main.ScreenToWorldPoint(v3_Screen);
        m_DestSprite = GetSpriteOfTouchPoint(new Vector3(v3_World.x, v3_World.y, 0));
    }

    GameObject GetSpriteOfTouchPoint(Vector3 point)
    {
        foreach(GameObject sprite in  m_SpriteArray)
        {
            if(sprite != null)
            {
                if(Mathf.Abs(point.x - sprite.transform.position.x) <  (float)(m_SpriteWidth / 2) / 100.0f
                    && Mathf.Abs(point.y - sprite.transform.position.y) <  (float)(m_SpriteHeight / 2) / 100.0f)
                {
                    return sprite;
                }
            }
        }

        return null;
    }

    void SwapSprite(GameObject srcObject, GameObject destObject)
    {       
        if(!srcObject || !destObject )
        {
            return;
        }

        int SrcRow, SrcCol, DestRow, DestCol;
        Vector3 SrcPos = srcObject.transform.position;
        Vector3 DestPos = destObject.transform.position;
        GetSpriteInfo(SrcPos, out SrcRow, out SrcCol);
        GetSpriteInfo(DestPos, out DestRow, out DestCol);

        if (false == CheckCanSwap(SrcRow, SrcCol, DestRow, DestCol))
        {
            return;
        }

        //满足交换条件就交换位置
        m_SpriteArray[SrcRow, SrcCol] = destObject;
        m_SpriteArray[DestRow, DestCol] = srcObject;

        //检测交换后能否在横纵方向上满足消除要求
        List<GameObject> RowChainList_Dest = new List<GameObject>();
        GetRowChain(SrcRow, SrcCol, RowChainList_Dest);

        List<GameObject> ColChainList_Dest = new List<GameObject>();
        GetColChain(SrcRow, SrcCol, ColChainList_Dest);

        List<GameObject> RowChainList_Src = new List<GameObject>();
        GetRowChain(DestRow, DestCol, RowChainList_Src);

        List<GameObject> ColChainList_Src = new List<GameObject>();
        GetColChain(DestRow, DestCol, ColChainList_Src);

        if (RowChainList_Dest.Count >= 3
            || ColChainList_Dest.Count >= 3
            || RowChainList_Src.Count >= 3
            || ColChainList_Src.Count >= 3)
        {
            //交换后能消除，就交换一次
            float swapTime = 0.2f;
            m_IsAnimating = true;
            srcObject.SendMessage("SetIsAnimating");
            destObject.SendMessage("SetIsAnimating");
            iTween.MoveTo(srcObject, iTween.Hash("time", swapTime, "position", DestPos, "easetype", iTween.EaseType.linear, "oncomplete", "AnimationEnd", "oncompletetarget", srcObject, "onupdate", "AnimationUpdate", "onupdatetarget", srcObject));
            iTween.MoveTo(destObject, iTween.Hash("time", swapTime, "position", SrcPos, "easetype", iTween.EaseType.linear, "oncomplete", "AnimationEnd", "oncompletetarget", destObject, "onupdate", "AnimationUpdate", "onupdatetarget", destObject));
        }
        else
        {
            //交换后不能消除，就交换一次再交换回来
            float swapTime = 0.2f;
            m_IsAnimating = true;
            srcObject.SendMessage("SetIsAnimating");
            destObject.SendMessage("SetIsAnimating");
            iTween.MoveTo(srcObject, iTween.Hash("time", swapTime, "position", DestPos, "easetype", iTween.EaseType.linear, "oncomplete", "AnimationEnd", "oncompletetarget", srcObject, "onupdate", "AnimationUpdate", "onupdatetarget", srcObject));
            iTween.MoveTo(destObject, iTween.Hash("time", swapTime, "position", SrcPos, "easetype", iTween.EaseType.linear, "oncomplete", "AnimationEnd", "oncompletetarget", destObject, "onupdate", "AnimationUpdate", "onupdatetarget", destObject));
            iTween.MoveTo(srcObject, iTween.Hash("time", swapTime, "position", SrcPos, "easetype", iTween.EaseType.linear, "oncomplete", "AnimationEnd", "oncompletetarget", srcObject, "onupdate", "AnimationUpdate", "onupdatetarget", srcObject, "delay", swapTime));
            iTween.MoveTo(destObject, iTween.Hash("time", swapTime, "position", DestPos, "easetype", iTween.EaseType.linear, "oncomplete", "AnimationEnd", "oncompletetarget", destObject, "onupdate", "AnimationUpdate", "onupdatetarget", destObject, "delay", swapTime));
        
            //交换回行列号
            m_SpriteArray[SrcRow, SrcCol] = srcObject;
            m_SpriteArray[DestRow, DestCol] = destObject;
        }
    }

    bool CheckCanSwap(int SrcRow, int SrcCol, int DestRow, int DestCol)
    {
        if (SrcRow == DestRow)
        {
            if (Mathf.Abs(SrcCol - DestCol) == 1)
            {
                return true;
            }
        }
        else if(SrcCol == DestCol)
        {
            if (Mathf.Abs(SrcRow - DestRow) == 1)
            {
                return true;
            }
        }

        return false;
    }
}
