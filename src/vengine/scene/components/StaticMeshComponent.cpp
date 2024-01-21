﻿#include "StaticMeshComponent.hpp"

#include "vengine/Engine.hpp"
#include "vengine/utils.hpp"
#include "vengine/drawing/Drawer.hpp"
#include "vengine/drawing/MaterialInstance.hpp"
#include "vengine/drawing/scene/SceneDrawer.hpp"
#include "vengine/scene/SceneObject.hpp"
#include <glm/gtx/transform.hpp>

namespace vengine::scene {
drawing::Mesh * StaticMeshComponent::GetMesh() const {
  return _mesh;
}

void StaticMeshComponent::SetMesh(drawing::Mesh *newMesh) {
  if(newMesh != nullptr) {
    if(!newMesh->IsUploaded()) {
      newMesh->Upload();
    }
  }
  _mesh = newMesh;
}

void StaticMeshComponent::Draw(drawing::SceneDrawer *renderer,
                               drawing::SceneFrameData *frameData) {
  if(_mesh == nullptr || !_mesh->IsUploaded()) {
    return;
  }
  const auto transform = GetWorldTransform();

  drawing::SceneDrawPushConstants pushConstants{};

  pushConstants.transformMatrix = transform.Matrix(); //glm::mat4{1.f};
  pushConstants.vertexBuffer = _mesh->gpuData.value().vertexBufferAddress;

  const auto cmd = frameData->GetCmd();
  
  const auto surfaces = _mesh->GetSurfaces();
  const auto materials = _mesh->GetMaterials();
  const auto surfaceMatSizeMatch = surfaces.size() == materials.size();
  utils::vassert(surfaceMatSizeMatch,"Surfaces and Materials Size Mismatch");
  
  for(auto i = 0; i < surfaces.size(); i++) {
    const auto [startIndex, count] = surfaces[i];
    const auto material = materials[i] == nullptr ? renderer->GetEngineRenderer()->GetDefaultCheckeredMaterial() : materials[i];

    material->Bind(frameData);
    
    cmd->bindIndexBuffer(_mesh->gpuData.value().indexBuffer.buffer,0,vk::IndexType::eUint32);

    material->PushConstant(frameData->GetCmd(),"vertexPushConstant",sizeof(drawing::SceneDrawPushConstants),&pushConstants);
    
    cmd->drawIndexed(count,1,startIndex,0,0);
  }
}
}
