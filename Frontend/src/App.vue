<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { initMap } from './scripts/map.ts'

const openMenu = ref(true)

onMounted(async () => {
  await initMap('map-canvas')
})
</script>

<style lang="scss">
@import './assets/styles/App.scss';
</style>

<template>
  <div class="above-content">
    <div class="box-news"><h1 class="placard-h1">News</h1></div>
  </div>
  <div class="below-content">
    <button v-if="!openMenu" @click="openMenu = true" class="button-power-outage-outside">P</button>
    <div class="box-power-outage" :class="{ 'is-hidden': !openMenu }">
      <div class="box-power-outage-header">
        <h2 class="box-power-outrage-h2">Bairros sem luz</h2>
        <button class="button-power-outage-inside" @click="openMenu = false">X</button>
      </div>
      <ul class="lista-bairros-sem-luz">
        <li class="lista-items-bairros-sem-luz"><strong>Bairro sem luz 1</strong></li>
        <li class="lista-items-bairros-sem-luz"><strong>Bairro sem luz 2</strong></li>
        <li class="lista-items-bairros-sem-luz"><strong>Bairro sem luz 3</strong></li>
        <li class="lista-items-bairros-sem-luz"><strong>Bairro sem luz 4</strong></li>
        <li class="lista-items-bairros-sem-luz"><strong>Bairro sem luz 5</strong></li>
      </ul>
    </div>
    <div class="box-map" id="map-canvas"><h1>Map</h1></div>
  </div>
</template>
