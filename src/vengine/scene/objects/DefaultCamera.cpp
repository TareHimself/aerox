﻿#include "DefaultCamera.hpp"

#include "vengine/Engine.hpp"
#include "vengine/input/InputManager.hpp"
#include "vengine/input/KeyInputEvent.hpp"
#include "vengine/math/constants.hpp"
#include "vengine/scene/Scene.hpp"

#include <SDL_keycode.h>

namespace vengine::scene {
SceneComponent *DefaultCamera::CreateRootComponent() {
  camera = AddComponent<CameraComponent>();
  return camera;
}

void DefaultCamera::Init(scene::Scene *outer) {
  SceneObject::Init(outer);

  const auto inputManager = GetInput();

  AddCleanup(inputManager->BindKey(SDLK_w, [=](const input::KeyInputEvent &e) {
                                     inputForward += 1;
                                     return true;
                                   }, [=](const input::KeyInputEvent &e) {
                                     inputForward -= 1;
                                     return true;
                                   }));

  AddCleanup(inputManager->BindKey(SDLK_s, [=](const input::KeyInputEvent &e) {
                                     inputForward -= 1;
                                     return true;
                                   }, [=](const input::KeyInputEvent &e) {
                                     inputForward += 1;
                                     return true;
                                   }));

  AddCleanup(inputManager->BindKey(SDLK_a, [=](const input::KeyInputEvent &e) {
                                     inputRight -= 1;
                                     return true;
                                   }, [=](const input::KeyInputEvent &e) {
                                     inputRight += 1;
                                     return true;
                                   }));
  AddCleanup(inputManager->BindKey(SDLK_d, [=](const input::KeyInputEvent &e) {
                                     inputRight += 1;
                                     return true;
                                   }, [=](const input::KeyInputEvent &e) {
                                     inputRight -= 1;
                                     return true;
                                   }));

  AddCleanup(inputManager->BindKey(SDLK_SPACE,
                                   [=](const input::KeyInputEvent &e) {
                                     bWantsToGoUp = true;
                                     return true;
                                   }, [=](const input::KeyInputEvent &e) {
                                     bWantsToGoUp = false;
                                     return true;
                                   }));

  this->SetWorldLocation({0, 0, -10.f});

  constexpr float mouseInputScale = 0.1f;
  AddCleanup(inputManager->OnAxis(input::MouseX,
                                  [=](const input::AxisInputEvent &e) {
                                    yaw += e.GetValue() * 1.f * mouseInputScale;

                                    UpdateRotation();
                                    return true;
                                  }));

  AddCleanup(inputManager->OnAxis(input::MouseY,
                                  [=](const input::AxisInputEvent &e) {
                                    pitch += e.GetValue() * mouseInputScale;
                                    UpdateRotation();
                                    return true;
                                  }));
}

void DefaultCamera::Update(float deltaTime) {
  SceneObject::Update(deltaTime);

  constexpr auto moveSpeed = 10.f;
  const auto forwardDelta = GetWorldRotation().Forward() * inputForward *
                            moveSpeed * deltaTime;
  const auto rightDelta = GetWorldRotation().Right() * inputRight * moveSpeed *
                          deltaTime;
  const math::Vector upDelta = bWantsToGoUp
                                 ? math::VECTOR_UP * moveSpeed * deltaTime
                                 : math::VECTOR_ZERO;
  const math::Vector deltaFinal = forwardDelta + rightDelta + upDelta;
  const auto worldLocation = GetWorldLocation();
  SetWorldLocation(worldLocation + deltaFinal);
}

void DefaultCamera::UpdateRotation() {
  SetWorldRotation(math::QUAT_ZERO.ApplyYaw(yaw).ApplyPitch(pitch));
  //setWorldRotation( glm::mat4(glm::angleAxis(glm::radians(yaw),math::Vector::Up)) * glm::mat4(glm::angleAxis(glm::radians(pitch),math::Vector::Right)));
}
}
