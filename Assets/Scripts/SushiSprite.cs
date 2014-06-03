using UnityEngine;
using System.Collections;

public class SushiSprite : MonoBehaviour
{
    public bool m_IsAnmationEnd;

    // Use this for initialization
    void Start()
    {
        m_IsAnmationEnd = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void AnimationUpdate()
    {
        m_IsAnmationEnd = false;

        //Debug.Log("m_IsAnmationEnd = false");
    }

    void AnimationEnd()
    {
        m_IsAnmationEnd = true;

        //Debug.Log("m_IsAnmationEnd = true");
    }

    public bool IsAnmationEnd()
    {
        return m_IsAnmationEnd;
    }

    public void SetIsAnimating()
    {
        m_IsAnmationEnd = false;
    }
}
