open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

type LoopState = 
| Update of GameTime
| Draw of GameTime

type XnaGame() as this =
    inherit Game()

    let initializeEvt = Event<_>()
    let loadEvt = Event<_>()
    let loopEvt = Event<_>()

    do this.Content.RootDirectory <- "XnaGameContent"
    let graphicsDeviceManager = new GraphicsDeviceManager(this)

    override game.Initialize() =
        graphicsDeviceManager.GraphicsProfile <- GraphicsProfile.HiDef
        graphicsDeviceManager.PreferredBackBufferWidth <- 640
        graphicsDeviceManager.PreferredBackBufferHeight <- 480
        graphicsDeviceManager.ApplyChanges() 
        game.IsFixedTimeStep <- false
        initializeEvt.Trigger()
        base.Initialize()

    override game.LoadContent() =
        loadEvt.Trigger()
        
    override game.Update gameTime = 
        loopEvt.Trigger (Update gameTime)

    override game.Draw gameTime = 
        loopEvt.Trigger (Draw gameTime)
        
    member game.InitializeAsync =  Async.AwaitEvent initializeEvt.Publish
    member game.LoadContentAsync =  Async.AwaitEvent loadEvt.Publish
    member game.LoopAsync =  Async.AwaitEvent loopEvt.Publish

let game = new XnaGame()

let gameWorkflow = async {
    do! game.InitializeAsync
    let spriteBatch = new SpriteBatch(game.GraphicsDevice)
    do! game.LoadContentAsync
    let sprite = game.Content.Load<Texture2D>("Sprite")

    let rec gameLoop x y dx dy = async {
        let! nextState = game.LoopAsync
        match nextState with
        | Update time -> 
            let dx = if x > 608.f || x < 0.f then -dx else dx
            let dy = if y > 448.f || y < 0.f then -dy else dy
            let x = x + dx
            let y = y + dy
            return! gameLoop x y dx dy
        | Draw time -> 
            game.GraphicsDevice.Clear(Color.CornflowerBlue)
            spriteBatch.Begin()
            spriteBatch.Draw(sprite, Vector2(x,y), Color.White)
            spriteBatch.End()
            return! gameLoop x y dx dy
    }
    return! gameLoop 0.f 0.f 4.f 4.f
}

gameWorkflow |> Async.StartImmediate

game.Run()
