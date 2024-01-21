﻿#pragma once
#include "GpuNative.hpp"
#include "types.hpp"
#include "vengine/Object.hpp"
#include "vengine/assets/Asset.hpp"
#include "vengine/containers/Array.hpp"

namespace vengine::drawing {
class Drawer;
}

namespace vengine::drawing {
class MaterialInstance;
}

namespace vengine::assets {
class AssetManager;
}

namespace vengine {
class Engine;
}

namespace vengine::drawing {
struct MeshSurface {
  uint32_t startIndex;
  uint32_t count;
};


class Mesh : public Object<Drawer>, public assets::Asset, public GpuNative {

protected:
  Array<Vertex> _vertices;
  Array<uint32_t> _indices;
  Array<MeshSurface> _surfaces;
  Array<MaterialInstance *> _materials;
public:
  Array<Vertex> GetVertices() const;
  Array<uint32_t> GetIndices() const;
  Array<MeshSurface> GetSurfaces() const;
  Array<MaterialInstance*> GetMaterials() const;


  void SetVertices(const Array<Vertex> &vertices);
  void SetIndices(const Array<uint32_t> &indices);
  void SetSurfaces(const Array<MeshSurface> &surfaces);
  
  std::optional<GpuMeshBuffers> gpuData;

  void SetMaterial(uint32_t index,MaterialInstance * material);
  void Upload() override;
  bool IsUploaded() const override;
  String GetName() const;

  void HandleDestroy() override;

  String GetSerializeId() override;

  void ReadFrom(Buffer &store) override;

  void WriteTo(Buffer &store) override;
};
}
