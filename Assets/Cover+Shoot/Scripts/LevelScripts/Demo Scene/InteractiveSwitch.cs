﻿using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
// This class is created for the example scene. There is no support for this script.
public class InteractiveSwitch : MonoBehaviour
{
    [System.Serializable]
    class MovePoint
    {
        [SerializeField]
        public Transform point;
        [SerializeField]
        public float move_speed;
        [SerializeField]
        public float rotate_speed;
        [SerializeField]
        public float delate_time;
    }

    [SerializeField]
    private MovePoint[] movingstate;

	public List<TargetHealth> targets;
	public bool startVisible;
	public InteractiveSwitch nextStage;
	public bool levelEnd;
	public AudioClip activateSound;

	private GameObject player;
	private TargetHealth boss;
	private int minionsDead = 0;
	private State currentState;

    private bool isnow = false;

	private TimeTrialManager timer;

    private ThirdPersonOrbitCam orbitcam;

    private int effectiveShooting;

    private Text bullet_num;

    [SerializeField]
    action movingtype;

    [SerializeField]
    action actiontype;

    private enum State
	{
		DISABLED,
		MINIONS,
		END
	}

	private void Awake()
	{
		player = GameObject.FindGameObjectWithTag("Player");
        effectiveShooting = targets.Count * 2;

        this.ToggleState(false, startVisible);
		timer = GameObject.Find("Timer").GetComponent<TimeTrialManager>();
        orbitcam = GameObject.Find("Main Camera").GetComponent<ThirdPersonOrbitCam>();
        bullet_num = GameObject.Find("bullet_num").GetComponent<Text>();

        if (levelEnd)
		{
			currentState = State.END;
		}
		else
			currentState = State.DISABLED;


        foreach (TargetHealth i in targets)
        {
            if (i.transform.parent.gameObject.GetComponent<obstacles_event>())
                i.transform.parent.gameObject.GetComponent<obstacles_event>().swich = false;
        }
	}

    private void Start()
    {
        timer.LevelTimerEvent += mandatory_nextStage;

        if (startVisible)
            Moving();
    }

    void Update()
	{
		switch (currentState)
		{
			case State.MINIONS:
				minionsDead = 0;
				foreach (TargetHealth target in targets)
				{
					if (!target.boss && target.IsDead)
					{
						minionsDead++;
					}
				}
				if (minionsDead == targets.Count)
				{
                    timer.EndLevelTimer();
                    this.ToggleState(false, false);
                    isnow = false;
                    if (nextStage)
                    {
                        nextStage.ToggleState(false, true);
                        nextStage.Moving();
                    }
                    bullet_num.text = string.Format("剩余{0}次射击机会", effectiveShooting * 1.5 - player.GetComponent<ShootBehaviour>().Level_shoot_num);
                }
				break;
		}

        

       

        if (!isnow)
            return;
        bullet_num.text = string.Format("剩余{0}次射击机会", effectiveShooting * 1.5 - player.GetComponent<ShootBehaviour>().Level_shoot_num);

        if (player.GetComponent<ShootBehaviour>().Level_shoot_num >= effectiveShooting * 1.5)
        {
            mandatory_nextStage();
        }
	}


 

    void mandatory_nextStage()
    {
        if (!isnow)
            return;
        timer.EndLevelTimer();
        this.ToggleState(false, false);
        isnow = false;
        if (nextStage)
        {
            nextStage.ToggleState(false, true);
            nextStage.Moving();
        }

    }

	public void ToggleState(bool active, bool visible)
	{
		if (active)
			currentState = State.MINIONS;
		else
			currentState = State.DISABLED;
		this.GetComponent<BoxCollider>().enabled = visible;
		this.GetComponent<MeshRenderer>().enabled = visible;
        
	}

	private void OnTriggerEnter(Collider other)
	{
        if (other.gameObject == player)
		{
			if (levelEnd)
			{
				timer.EndTimer();
                timer.EndLevelTimer();
				ToggleState(false, false);

				if (nextStage)
				{
					nextStage.ToggleState(false, true);
				}
			}
			else
			{
				if(GameManager._Instance.timeTrial && !timer.IsRunning)
				{
					timer.StartTimer();
                    
				}
				ToggleState(true, false);
                timer.StartLevelTimer();
                foreach (TargetHealth i in targets)
                {
                    if (i.transform.parent.gameObject.GetComponent<obstacles_event>())
                        i.transform.parent.gameObject.GetComponent<obstacles_event>().swich = true;
                }
                switch (actiontype)
                {
                    case action.StandUp:
                        orbitcam.StandUp();
                        break;
                    case action.SquatDown:
                        orbitcam.SquatDown();
                        break;
                }
                player.GetComponent<ShootBehaviour>().Level_shoot_num = 0;
                isnow = true;
                foreach (TargetHealth target in targets)
				{
					if (!target.boss)
					{
						target.Revive();
					}
					else
					{
						boss = target;
						boss.Kill();
					}
				}
			}
			AudioSource.PlayClipAtPoint(activateSound, transform.position + Vector3.up);
		}
	}

    public void Moving()
    {
        StartCoroutine(move_to_point());
    }

    IEnumerator move_to_point()
    {
        switch (movingtype)
        {
            case action.StandUp:
                orbitcam.StandUp();
                break;
            case action.SquatDown:
                orbitcam.SquatDown();
                break;
        }
        foreach (MovePoint i in movingstate)
        {
            while (Vector3.Distance(player.transform.position, i.point.position) > 0.1f)
            {
                player.transform.position = Vector3.Lerp(player.transform.position,i.point.position,i.move_speed * Time.deltaTime);
                player.transform.rotation = Quaternion.Lerp(player.transform.rotation,i.point.rotation,i.rotate_speed * Time.deltaTime);
                yield return null;
            }
        }
        yield return null;
    }

}

