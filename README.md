<p align="center">
<h1 align="center">Poly Robots</h1>
<img alt="header image" src="https://static.poly.pizza/press/1.jpg">
</p>

## 💡 What?
Poly Robots is a demo of the [poly.pizza](https://poly.pizza) api. Poly pizza is a website that hosts thousands of free low poly models under CC0 and CC-BY licenses. 
The site has an API that allows for real time loading of models into a game engine. You can view the [API docs here](https://poly.pizza/docs/api)

This example shows how you might load, scale, position and generate colliders in the Untiy game engine (it's harder than you think!)

## 🖥 Install
- Make sure git is installed
- Load up the project in Unity 2020.2.26f1 and [install glTFast](https://github.com/atteneder/glTFast#installing).
- Get your API key from [here](https://poly.pizza/settings/api) (you'll need an account).
- Paste your api key into the API manager on the spawner object
![unity screenshot](https://i.imgur.com/2qeaLYl.png)

## 🍕 Using it in your own projects
You can use the API manager in your own projects to load models from the poly.pizza api without reinventing the wheel.
- First make sure glTFast is installed in your project as well as [Unitask](https://github.com/Cysharp/UniTask)
- Grab the PizzaBox directory and put it in your project

The API manager provides a few methods for loading and rendering models.
First you'll need to decide what models you'd like to load. There's a few methods for this:
- `GetPopular(int limit)` - Get an array of the most popular models on the site up to the `limit` you specify.

- `GetExactModel(string keyword)` - Search for a model matching your `keyword` exactly. This is useful if you know what you want to load (like an apple or a monkey) but don't know the exact model.

- `GetModelByID(string id)` - Get a model by it's unique ID. (crazy ik)

Once you've got the model data from the api you can make it into a gameObject with the `MakeModel(Model model, float scale = 1, bool positionCenter = false)` Positioning the model is tricky since the models origin can be totally fucked. Check out `spawner.cs` to see how to position objects where you want them. 

### 🤓 Deep lore 

The Robot in this scene is a shittily rigged version of [Doodle](https://poly.pizza/m/93NCs2zpMq) by Jeremy Edelblut [CC-BY]. A version of this lil robt guy was used to promo the Google Poly api, which poly pizza was created to replace after it shutdown. I'd like to think after poly shutdown he quit his corporate job, got a sick paint job and is living his best life.

![corpobots](https://storage.googleapis.com/gweb-uniblog-publish-prod/original_images/4_FiJeTKv.png)

![doodleboy](https://i.imgur.com/8Kea73n.jpeg)
