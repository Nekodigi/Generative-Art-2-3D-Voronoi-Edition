using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageFill : MonoBehaviour
{
    Texture2D texture;
    Image image;
    RectTransform rt;
    float scale = 1f;
    // Start is called before the first frame update
    void Start()
    {
        image = gameObject.GetComponent<Image>();
        rt = gameObject.GetComponent<RectTransform>();
        if (image == null) Debug.Log(gameObject.name);
        texture = image.sprite.texture;
    }

    // Update is called once per frame
    void Update()
    {
        image.sprite = Sprite.Create(texture, new Rect(texture.width/2.0f-rt.sizeDelta.x * scale/2.0f, texture.height / 2.0f - rt.sizeDelta.y * scale / 2.0f, rt.sizeDelta.x * scale, rt.sizeDelta.y* scale), Vector2.zero);
        
    }
}
