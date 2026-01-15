using System.Collections;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] GameObject AlphaBlock;
    private Coroutine hideAlphaBlock;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void ShowAlphaBlock()
    {
        if (AlphaBlock == null) return;
        if (hideAlphaBlock != null)
        {
            StopCoroutine(hideAlphaBlock);
            hideAlphaBlock = null;
        }
        AlphaBlock.SetActive(true);
        hideAlphaBlock = StartCoroutine(HideAlphaBlock());
    }

    IEnumerator HideAlphaBlock()
    {
        yield return new WaitForSeconds(1.5f);
        AlphaBlock.SetActive(false);
    }
}
