using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ThoughtBubble : MonoBehaviour
{
    public enum Thought
    {
        Food,
        Water,
        Rest,
        Happy,
        Hurt
    }

    [System.Serializable]
    struct ThoughtBubbleIcons
    {
        public Thought thought;
        public Sprite sprite;
    }

    [SerializeField]
    Image bubbleImage;

    [SerializeField]
    Image iconImage;

    [SerializeField]
    List<ThoughtBubbleIcons> icons;

    public void SetThought(Thought thought)
    {
        bubbleImage.gameObject.SetActive(true);
        iconImage.sprite = icons.Where(i => i.thought == thought)
                                .Select(s => s.sprite)
                                .FirstOrDefault();
    }

    public void DisableThought()
    {
        bubbleImage.gameObject.SetActive(false);
    }
}