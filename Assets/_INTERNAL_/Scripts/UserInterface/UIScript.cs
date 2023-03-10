using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("")]
public class UIScript : MonoBehaviour
{
	/// <summary>
	/// Возможные положения персонажа с корзиной
	/// </summary>
	[Flags]
	private enum PersonPosition : byte
	{
		None = 0,
		Left =	 0b0000_0001,
		Right =	 0b0000_0010,
		Top =	 0b0000_0100,
		Bottom = 0b0000_1000,
		TopLeft = Left | Top,
		BottomRight = Right | Bottom,
		TopRight = Right | Top,
		BottomLeft = Left | Bottom,
	}


    #region 'Поля и константы'

    [Header("Configuration")]
	public Players numberOfPlayers = Players.OnePlayer;

	public GameType gameType = GameType.Score;

	// If the scoreToWin is -1, the game becomes endless (no win conditions, but you could do game over)
	public int scoreToWin = 100000;
	[Header("Кол-во очков, чтобы заработать +1 жизнь")]
    public int scoreToNewLife = 5000;
    [Header("Метка для вывода итогового кол-ва очков")]
    [SerializeField] private Text totalScore;
    [Header("Эффект при победе")]
    [SerializeField] private GameObject winEffect;
    [Header("Звук при победе")]
    [SerializeField] private AudioClip winSound;
	[Header("Стартовый экран игры")]
    [SerializeField] private GameObject startPanel;
    [Header("Игрок и корзина для яиц")]
    [SerializeField] private GameObject person;
    [SerializeField] private GameObject basket;
    [Header("Границы интервала случайного времени между генерацией нового яйца")]
    [SerializeField] private float minNestSpawnInterval = 0.2f;
    [SerializeField] private float maxNestSpawnInterval = 2f;

    [Header("Время в секундах между переключением гнёзд")]
    [SerializeField] public float SwitchInterval;    

    [Header("References (don't touch)")]
	//Right is used for the score in P1 games
	public Text[] numberLabels = new Text[2];
	public Text rightLabel, leftLabel;
	public Text winLabel;
	public GameObject statsPanel, gameOverPanel, winPanel;
	public Transform inventory;
	public GameObject resourceItemPrefab;

    // Internal variables to keep track of score, health, and resources, win state
    private int[] scores = new int[2];
	private int[] playersHealth = new int[2];
    //holds a reference to all the resources collected, and to their UI
    private Dictionary<int, ResourceStruct> resourcesDict = new Dictionary<int, ResourceStruct>(); 
    private bool gameOver = false;
	private HealthSystemAttribute healthSystem;
	private ObjectCreatorArea[] creators;
    private int startHealth;
	private int lastScoreToLifesCount = 0;
	private AudioSource soundEffectsPlayer;
	/// <summary>
	/// Тэг объектов, которые скрыты до начала игры
	/// </summary>
	private const string LEVEL_TAG_NAME = "Level";    

    /// <summary>
    /// Список объектов, которые скрыты до начала игры
    /// </summary>
    private IEnumerable<GameObject> levelObject;
	private PersonPosition currentPosition = PersonPosition.None;
    private float worldCenterX;
    private float personWidth;
    private float personShift;
    private float basketShiftX;
    private float basketShiftY;

    #endregion 'Поля и константы'


    /// <summary>
    /// Сумма очков набранная всеми игроками
    /// </summary>
    public int TotalScore
	{
		get
		{
			return scores.Sum();
		}
	}

    //this gets changed when the game is won OR lost
    public bool IsGameOver => gameOver;

    private void Awake()
    {
        healthSystem = GameObject.FindObjectOfType<HealthSystemAttribute>();
		startHealth = healthSystem.health;
        creators = GameObject.FindObjectsOfType<ObjectCreatorArea>();
		soundEffectsPlayer = GetComponent<AudioSource>();
		levelObject = GameObject.FindGameObjectsWithTag(LEVEL_TAG_NAME);
		foreach (GameObject levelObject in levelObject)
		{
			levelObject.SetActive(false);
		}
		person.SetActive(false);
        basket.SetActive(false);
        worldCenterX = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, 0, 0)).x;
		personWidth = person.GetComponent<SpriteRenderer>().bounds.size.x;
		personShift = 5 * personWidth / 4;
        basketShiftX = 4 * personWidth / 5;
		basketShiftY = 5 * person.GetComponent<SpriteRenderer>().bounds.extents.y / 4;
        gameOver = true;		
    }

    private void Start()
	{        
        if (numberOfPlayers == Players.OnePlayer)
		{
			// No setup needed
		}
        else if (gameType == GameType.Score)
        {
            // Show the 2-player score interface
            rightLabel.text = leftLabel.text = "Score";

            // Show the score as 0 for both players
            numberLabels[0].text = numberLabels[1].text = "0";
            scores[0] = scores[1] = 0;
        }
        else
        {
            // Show the 2-player life interface
            rightLabel.text = leftLabel.text = "Life";

            // Life will be provided by the PlayerHealth components
        }
		lastScoreToLifesCount = 0;
		// stops all creators except one
		creators.ToList().ForEach(creator => creator.enabled = false);
		StartCoroutine(AllowRandomCreator());
    }

	private IEnumerator AllowRandomCreator()
	{
		while (!gameOver)
		{
			int randIndex = UnityEngine.Random.Range(0, creators.Length);
			//Debug.Log($"Starting creator {creators[randIndex]}...");
			for (int i = 0; i < creators.Length; i++)
			{
				var cr = creators[i];
				cr.SpawnInterval = UnityEngine.Random.Range(minNestSpawnInterval, maxNestSpawnInterval);
				cr.enabled = i == randIndex;
			}
			yield return new WaitForSeconds(SwitchInterval);
		}
    }

    private void Update()
    {
		if (gameOver && Input.GetKeyUp(KeyCode.R))
		{
			Restart();
		}
		else if (!gameOver)
		{
            var direction = PersonPosition.None;
            if (Input.GetKeyDown(KeyCode.RightArrow) && !currentPosition.HasFlag(PersonPosition.Right))
			{
                // переместить корзину Вправо на текущей высоте
                direction = PersonPosition.Right;
			}
            else if (Input.GetKeyDown(KeyCode.LeftArrow) && !currentPosition.HasFlag(PersonPosition.Left))
            {
                // переместить корзину Влево на текущей высоте
                direction = PersonPosition.Left;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow) && !currentPosition.HasFlag(PersonPosition.Top))
            {
                // переместить корзину Вверх на текущей стороне
                direction = PersonPosition.Top;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow) && !currentPosition.HasFlag(PersonPosition.Bottom))
            {
                // переместить корзину Вниз на текущей стороне
                direction = PersonPosition.Bottom;
            }
            else if (Input.GetKeyDown(KeyCode.Q) && currentPosition != PersonPosition.TopLeft)
            {
                // переместить корзину в Верхний Левый угол
                direction = PersonPosition.TopLeft;
            }
            else if (Input.GetKeyDown(KeyCode.A) && currentPosition != PersonPosition.BottomLeft)
            {
				// переместить корзину в Нижний Левый угол
				direction = PersonPosition.BottomLeft;
            }
            else if (Input.GetKeyDown(KeyCode.E) && currentPosition != PersonPosition.TopRight)
            {
                // переместить корзину в Верхний Правый угол
                direction = PersonPosition.TopRight;
            }
            else if (Input.GetKeyDown(KeyCode.D) && currentPosition != PersonPosition.BottomRight)
            {
				// переместить корзину в Нижний Правый угол
				direction = PersonPosition.BottomRight;
            }
			if (direction != PersonPosition.None)
			{
				MovePerson(direction);
			}
        }
    }

	/// <summary>
	/// Метод перемещения персонажа и/или корзины в указанное положение
	/// </summary>
	/// <param name="pos">
	/// Новое положение или его элемент
	/// </param>
    private void MovePerson(PersonPosition pos)
    {
		if (currentPosition == pos)
		{
			return;
		}
        // если в новой позиции задано только направление (ВЛЕВО, ВПРАВО, ВВЕРХ, ВНИЗ), то просто добавляем его к текущей позиции
        var persPos = person.transform.position;
		var basketPos = basket.transform.position;
        switch (pos)
		{
            case PersonPosition.Right:
            case PersonPosition.Left:
            {
                var sign = pos == PersonPosition.Right ? 1 : -1;
                persPos.x = worldCenterX + sign * this.personShift;
				if (currentPosition.HasFlag(PersonPosition.Top))
				{
					basketPos.x = persPos.x;
                    pos |= PersonPosition.Top;
                }
				else
				{
                    basketPos.x = persPos.x + sign * this.basketShiftX;
                    pos |= PersonPosition.Bottom;
                }
                break;
            }
			case PersonPosition.Top:
			{
				basketPos.x = persPos.x;
				basketPos.y = persPos.y + basketShiftY;
				pos = (currentPosition.HasFlag(PersonPosition.Left) ? PersonPosition.Left : PersonPosition.Right) | pos;
				break;
			}
			case PersonPosition.Bottom:
            {
				var sign = currentPosition.HasFlag(PersonPosition.Right) ? 1 : -1;
                basketPos.x = persPos.x + sign * this.basketShiftX;
                basketPos.y = persPos.y;
                pos = (currentPosition.HasFlag(PersonPosition.Left) ? PersonPosition.Left : PersonPosition.Right) | pos;
                break;
            }
            case PersonPosition.TopLeft:
            case PersonPosition.TopRight:
            case PersonPosition.BottomLeft:
            case PersonPosition.BottomRight:
            {
                var sign = pos.HasFlag(PersonPosition.Right) ? 1 : -1;
                persPos.x = worldCenterX + sign * this.personShift;
				if (pos.HasFlag(PersonPosition.Top))
				{
					basketPos.x = persPos.x;
					basketPos.y = persPos.y + basketShiftY;
				}
				else
				{
					basketPos.x = persPos.x + sign * this.basketShiftX;
					basketPos.y = persPos.y;
				}
                break; 
			}
        }
        person.transform.position = persPos;
        basket.transform.position = basketPos;
        currentPosition = pos;
    }

    /// <summary>
    /// Метод перезапуска игры
    /// </summary>
    /// <remarks>
    /// по кнопке «Начать заново»,
    /// по клавише «R».
    /// </remarks>
    public void Restart()
    {
        statsPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        startPanel.SetActive(false);
		if (healthSystem.health <= 0)
		{
			healthSystem.ModifyHealth(startHealth);
		}
		RemoveAllPoints();
        gameOver = false;
        Start();
		Camera.main.gameObject.GetComponent<AudioSource>().Play();		
        levelObject.ToList().ForEach(x => x.SetActive(true));
		MovePerson(PersonPosition.BottomRight);
        person.SetActive(true);
        basket.SetActive(true);
    }

    //version of the one below with one parameter to be able to connect UnityEvents
    public void AddOnePoint(int playerNumber)
	{
		AddPoints(playerNumber, 1);
	}

	/// <summary>
	/// Добавляет указанное кол-во очков для первого Игрока
	/// </summary>
	/// <param name="amount"></param>
	public void AddPoints(int amount)
	{
		AddPoints(0, amount);
	}


    public void AddPoints(int playerNumber, int amount = 1)
	{
		scores[playerNumber] += amount;

		var scoreToLifesCount = scores[playerNumber] / scoreToNewLife;
		if (scoreToLifesCount > lastScoreToLifesCount && healthSystem.health < startHealth)
		{
			lastScoreToLifesCount = scoreToLifesCount;			
			soundEffectsPlayer?.Play();
            healthSystem.ModifyHealth(1);
        }

        if (numberOfPlayers == Players.OnePlayer)
		{
			numberLabels[1].text = scores[playerNumber].ToString(); //with one player, the score is on the right
		}
		else
		{
			numberLabels[playerNumber].text = scores[playerNumber].ToString();
		}

		if(gameType == GameType.Score
			&& scores[playerNumber] >= scoreToWin)
		{
			GameWon(playerNumber);
		}
	}

	//currently unused by other Playground scripts
	public void RemoveOnePoint(int playerNumber)
	{
		scores[playerNumber]--;

		if(numberOfPlayers == Players.OnePlayer)
		{
			numberLabels[1].text = scores[playerNumber].ToString(); //with one player, the score is on the right
		}
		else
		{
			numberLabels[playerNumber].text = scores[playerNumber].ToString();
		}
	}

	public void RemoveAllPoints()
	{
		for (int i = 0; i < scores.Length; i++)
		{
			AddPoints(i, -scores[i]);
		}
	}


	public void GameWon(int playerNumber)
	{
		// only set game over UI if game is not over
	    if (!gameOver)
	    {
			gameOver = true;
			winLabel.text = "Player " + ++playerNumber + " wins!";
			statsPanel.SetActive(false);
			winPanel.SetActive(true);            
            soundEffectsPlayer?.PlayOneShot(winSound);
			Instantiate(winEffect);
            person.SetActive(false);
            basket.SetActive(false);
            StopCoroutine(AllowRandomCreator());
        }
	}



	public void GameOver(int playerNumber)
	{
        // only set game over UI if game is not over
	    if (!gameOver)
	    {
			gameOver = true;
	        statsPanel.SetActive(false);
	        gameOverPanel.SetActive(true);
			// По ТЗ уничтожаем все объекты на экране при конце Игры
            GameObject.FindGameObjectsWithTag(creators.First().prefabToSpawn.tag).ToList().ForEach(t => 
				Destroy(t.gameObject)
			);
            person.SetActive(false);
			basket.SetActive(false);
            totalScore.text = TotalScore.ToString();
			StopCoroutine(AllowRandomCreator());
        }
    }



	public void SetHealth(int amount, int playerNumber)
	{
		playersHealth[playerNumber] = amount;
		numberLabels[playerNumber].text = playersHealth[playerNumber].ToString();
	}



	public void ChangeHealth(int change, int playerNumber)
	{
		SetHealth(playersHealth[playerNumber] + change, playerNumber);

		if(gameType != GameType.Endless
			&& playersHealth[playerNumber] <= 0)
		{
			GameOver(playerNumber);
		}

	}



	//Adds a resource to the dictionary, and to the UI
	public void AddResource(int resourceType, int pickedUpAmount, Sprite graphics)
	{
		if(resourcesDict.ContainsKey(resourceType))
		{
			//update the dictionary key
			int newAmount = resourcesDict[resourceType].amount + pickedUpAmount;
			resourcesDict[resourceType].UIItem.ShowNumber(newAmount);
			resourcesDict[resourceType].amount = newAmount;
		}
		else
		{
			//create the UIItemScript and display the icon
			UIItemScript newUIItem = Instantiate<GameObject>(resourceItemPrefab).GetComponent<UIItemScript>();
			newUIItem.transform.SetParent(inventory, false);

			resourcesDict.Add(resourceType, new ResourceStruct(pickedUpAmount, newUIItem));

			resourcesDict[resourceType].UIItem.ShowNumber(pickedUpAmount);
			resourcesDict[resourceType].UIItem.DisplayIcon(graphics);
		}
	}


	//checks if a certain resource is in the inventory, in the needed quantity
	public bool CheckIfHasResources(int resourceType, int amountNeeded = 1)
	{
		if(resourcesDict.ContainsKey(resourceType))
		{
			if(resourcesDict[resourceType].amount >= amountNeeded)
			{
				return true;
			}
			else
			{
				//not enough
				return false;
			}
		}
		else
		{
			//resource not present
			return false;
		}
	}


	//to use only before checking that the resource is in the dictionary
	public void ConsumeResource(int resourceType, int amountNeeded = 1)
	{
		resourcesDict[resourceType].amount -= amountNeeded;
		resourcesDict[resourceType].UIItem.ShowNumber(resourcesDict[resourceType].amount);
	}


	public enum Players
	{
		OnePlayer = 0,
		TwoPlayers
	}

	public enum GameType
	{
		Score = 0,
		Life,
		Endless
	}
}



//just a virtual representation of the resources for the private dictionary
public class ResourceStruct
{
	public int amount;
	public UIItemScript UIItem;

	public ResourceStruct(int a, UIItemScript uiRef)
	{
		amount = a;
		UIItem = uiRef;
	}
}