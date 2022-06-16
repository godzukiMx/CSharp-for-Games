using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Investigar sobre singleton con multijugadores.

public class PlayerController : MonoBehaviour
{

    public static PlayerController controllerInstace;

    //Variables del movimiento del personaje
    public float jumpForce = 7f;
    public float runningSpeed = 4f;
    Rigidbody2D rigidBody;
    Animator animator;
    SpriteRenderer render;
    Vector3 startPosition;
    
    const string STATE_ALIVE =  "isAlive";
    const string STATE_ON_THE_GROUND = "isOnTheGround";
    const string IS_STATIC = "isStatic";

    private int healthPoints, manaPoints;
    public const int INITIAL_HEALTH = 100, INITIAL_MANA = 15, MAX_HEALT = 200, MAX_MANA = 30, MIN_HEALTH = 10, MIN_MANA = 0;
    public LayerMask groundMask;

    void Awake() 
    {
        if (controllerInstace == null)
        {
            controllerInstace = this;
        }

        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        render = GetComponent<SpriteRenderer>();
    }
    // Start is called before the first frame update
    void Start()
    {
        startPosition = this.transform.position;
    }
    // Inicio y reinicio del juego
    public void StarGame(){
        animator.SetBool(STATE_ALIVE, true);
        animator.SetBool(STATE_ON_THE_GROUND, true);
        animator.SetBool(IS_STATIC, true);

        healthPoints = INITIAL_HEALTH;
        manaPoints = INITIAL_MANA;

        Invoke("RestardPosition", 0.2f);

    }

    void RestardPosition(){
        this.transform.position = startPosition;
        this.rigidBody.velocity = Vector2.zero;

        GameObject mainCamera = GameObject.Find("Main Camera");
        mainCamera.GetComponent<CameraFollow>().ResetCameraPosition();
    }


    // Update is called once per frame
    void Update()
    {   // Detecta si se presiono la tecla para saltar y manda llamar el metodo de salto
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        // CAmbios booleanos para indicar si esta en el suelo o si esta estatico y cambiar las animaciones necesarias
        animator.SetBool(STATE_ON_THE_GROUND, isTouchingTheGround());
        animator.SetBool(IS_STATIC, isStatic());

        Debug.DrawRay(this.transform.position, Vector2.down * 1.5f, Color.red);
    }

    // Update fijo, no se llama cada frame
     void FixedUpdate()    
    {   if(GameManager.sharedInstance.currentGameState == GameState.inGame)
        {// Detecta el movimiento en base al eje indicado, agregando velocidad para mover al personaje
            rigidBody.velocity = new Vector2(Input.GetAxis("Horizontal") * runningSpeed, rigidBody.velocity.y);
            rigidBody.gravityScale = 1; 
            
            // Identifica hacia donde esta volteando el personaje y grira horizontalmente el sprite
            if (Input.GetAxis("Horizontal") < 0)
            {
                render.flipX = true;
            }
            if (Input.GetAxis("Horizontal") > 0)
            {
                render.flipX = false;
            }
        }else{ // Si no se esta dentro de la partida
            rigidBody.velocity = new Vector2(0, rigidBody.velocity.y);
            rigidBody.gravityScale = 0;
        }
    }

    // Función  para realizar el salto
    void Jump()
    {
        if(GameManager.sharedInstance.currentGameState == GameState.inGame){
            if(isTouchingTheGround())
            {
            // Agregamos fuerzas vertical e impulso
            rigidBody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }
    }

        // Indica si el personaje toca o no el suelo
        bool isTouchingTheGround()
        {
            if (Physics2D.Raycast(this.transform.position,
                                  Vector2.down,
                                  1.5f,
                                  groundMask)){
                return true;                        
            }else{
                return false;
            }
        }

        // Indica si el personaje esta en movimiento o estatico
        bool isStatic()
        {
            if (Input.GetAxis("Horizontal") == 0)
            {
                return true;
            }else
            {
                return false;
            }
        }
        // detecta si el jugador muere y cambia el estado de juego a gameOver
        public void Die(){
            this.animator.SetBool(STATE_ALIVE, false);
            GameManager.sharedInstance.GameOver();
        }

        // Metodo para indicar si se recolecto vida y toparla al maximo de esta
        public void CollectHealth (int points)
        {
            this.healthPoints += points;
            if(this.healthPoints >= MAX_HEALT)
            {
                this.healthPoints = MAX_HEALT;
            }
        }

        // Metodo para indicar si se recolecto mana y toparlo al maximo de este
        public void CollectMana(int points)
        {
            this.manaPoints += points;
            if(this.manaPoints >= MAX_MANA)
            {
                this.manaPoints = MAX_MANA;
            }
        }

        // Metodo para llamar y mostrar en pantalla cuanta vida tiene el jugador
        public int GetHealth()
        {
            return healthPoints;
        }

        // Metodo para llamar y mostrar en pantalla cuantO mana tiene el jugador
        public int GetMana()
        {
            return manaPoints;
        }
}