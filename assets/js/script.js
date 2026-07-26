/* ---------------- DATA ---------------- */
const categories = [
  {id:'aricilar',  name:'Arıçılar',  icon:'🐝', desc:'bal, mum, ana arı'},
  {id:'akinciler', name:'Əkinçilər', icon:'🌾', desc:'taxıl, tərəvəz'},
  {id:'maldarlar', name:'Maldarlar', icon:'🐄', desc:'süd, ət, sağmal'},
  {id:'bagcilar',  name:'Bağçılar',  icon:'🍏', desc:'meyvə, tinglər'},
];

const advicePosts = [
  {cat:'aricilar',  author:'Rəşad Ə.', title:'Pətəkdə güvə görünüb, nə etməli?', body:'İlk dəfə arıçılıqla məşğul oluram, pətəyin künclərində güvə izlərinə rast gəldim...', answers:6},
  {cat:'akinciler', author:'Nərmin H.', title:'Buğdada sarı pas xəstəliyi', body:'Bu il yarpaqlarda sarı ləkələr artıb, hansı preparatı tövsiyə edərdiniz?', answers:11},
  {cat:'maldarlar', author:'Tural M.', title:'Sağmal inəkdə süd azalması', body:'Son həftə süd verimi kəskin azalıb, yem norması ilə bağlı ola bilərmi?', answers:4},
  {cat:'bagcilar',  author:'Aygün S.', title:'Alma ağacında budama vaxtı', body:'Payız yoxsa erkən yaz — hansı budama üçün daha münasibdir?', answers:8},
  {cat:'aricilar',  author:'Kamran V.', title:'Qış üçün pətəyi necə hazırlamalı', body:'Şimal rayonlarında qışlama üçün əlavə izolyasiya lazımdırmı?', answers:5},
  {cat:'akinciler', author:'Ləman Q.', title:'Damcı suvarma sistemi sərfəlidirmi?', body:'Kiçik sahə üçün quraşdırma dəyəri neçə mövsümə çıxır?', answers:9},
];

const listings = [
  {cat:'aricilar',  title:'Təbii bal (1 kq)', price:'18 ₼', seller:'Rəşad Ə.', unit:'kq başına'},
  {cat:'aricilar',  title:'Ana arı (Karnika)', price:'35 ₼', seller:'Kamran V.', unit:'ədəd'},
  {cat:'akinciler', title:'Buğda toxumu (elit)', price:'0.9 ₼', seller:'Nərmin H.', unit:'kq'},
  {cat:'akinciler', title:'Pomidor tingi', price:'0.6 ₼', seller:'Ləman Q.', unit:'kök'},
  {cat:'maldarlar', title:'Təzə inək südü', price:'2.2 ₼', seller:'Tural M.', unit:'litr'},
  {cat:'maldarlar', title:'Ev pendiri', price:'12 ₼', seller:'Tural M.', unit:'kq'},
  {cat:'bagcilar',  title:'Alma tingi (Simirenko)', price:'8 ₼', seller:'Aygün S.', unit:'ədəd'},
  {cat:'bagcilar',  title:'Təzə gilas (mövsüm)', price:'9 ₼', seller:'Aygün S.', unit:'kq'},
];

const tickerFeed = [
  {icon:'🐝', text:'Rəşad Ə. yeni sual yazdı: "Pətəkdə güvə görünüb"', meta:'Arıçılar · 4 dəq əvvəl'},
  {icon:'🌾', text:'Nərmin H. buğda toxumu elanını yenilədi', meta:'Əkinçilər · 12 dəq əvvəl'},
  {icon:'🐄', text:'Tural M. təzə süd elanı əlavə etdi', meta:'Maldarlar · 25 dəq əvvəl'},
  {icon:'🍏', text:'Aygün S. sualına 3 yeni cavab gəldi', meta:'Bağçılar · 33 dəq əvvəl'},
  {icon:'🐝', text:'Kamran V. ana arı satışa çıxardı', meta:'Arıçılar · 47 dəq əvvəl'},
  {icon:'🌾', text:'Ləman Q. damcı suvarma haqqında məsləhət istədi', meta:'Əkinçilər · 1 saat əvvəl'},
];

let activeCat = 'all';
let activeAdviceTab = 'all';

/* ---------------- RENDER ---------------- */
function catName(id){ return categories.find(c=>c.id===id)?.name || id; }
function catIcon(id){ return categories.find(c=>c.id===id)?.icon || '•'; }

function renderStamps(){
  const row = document.getElementById('stampRow');
  const all = [{id:'all',name:'Hamısı',icon:'🌐',desc:'bütün kateqoriyalar'}, ...categories];
  row.innerHTML = all.map(c => `
    <div class="stamp ${activeCat===c.id?'active':''}" onclick="setCategory('${c.id}')">
      <div class="icon">${c.icon}</div>
      <div class="txt"><b>${c.name}</b><span>${c.desc}</span></div>
    </div>
  `).join('');
}

function renderAdviceTabs(){
  const bar = document.getElementById('adviceTabs');
  const all = [{id:'all',name:'Hamısı'}, ...categories];
  bar.innerHTML = all.map(c=>`<button class="tab ${activeAdviceTab===c.id?'active':''}" onclick="setAdviceTab('${c.id}')">${c.name}</button>`).join('');
}

function renderAdvice(){
  const grid = document.getElementById('adviceGrid');
  const filterCat = activeAdviceTab !== 'all' ? activeAdviceTab : activeCat;
  const items = advicePosts.filter(p => filterCat==='all' || p.cat===filterCat);
  grid.innerHTML = items.map((p,i)=>`
    <div class="card ${i%2? 'tilt-b':'tilt-a'}">
      <span class="cat-tag">${catIcon(p.cat)} ${catName(p.cat)}</span>
      <h3>${p.title}</h3>
      <p>${p.body}</p>
      <div class="row">
        <div class="who"><div class="avatar">${p.author[0]}</div>${p.author}</div>
        <span>${p.answers} cavab</span>
      </div>
    </div>
  `).join('') || `<p style="color:var(--olive)">Bu kateqoriyada hələ sual yoxdur. İlk sualı sən yaz!</p>`;
}

function renderMarket(){
  const grid = document.getElementById('marketGrid');
  const items = listings.filter(l => activeCat==='all' || l.cat===activeCat);
  grid.innerHTML = items.map((l,i)=>`
    <div class="card ${i%2? 'tilt-a':'tilt-b'}">
      <span class="cat-tag">${catIcon(l.cat)} ${catName(l.cat)}</span>
      <h3>${l.title}</h3>
      <p>${l.seller} tərəfindən, ${l.unit}</p>
      <div class="row">
        <span class="price">${l.price}</span>
        <button class="btn btn-sky btn-small" onclick="openModal('buy','${l.title.replace(/'/g,"")}','${l.price}')">Al</button>
      </div>
    </div>
  `).join('') || `<p style="color:var(--olive)">Bu kateqoriyada hələ elan yoxdur.</p>`;
}

function renderFooterCats(){
  document.getElementById('footerCats').innerHTML = categories.map(c=>`<li><a href="#kateqoriyalar" onclick="setCategory('${c.id}')">${c.name}</a></li>`).join('');
}

function renderTicker(){
  const track = document.getElementById('tickerTrack');
  const html = tickerFeed.map(t=>`
    <div class="tick-item"><div class="pin">${t.icon}</div><div><b>${t.text}</b><span class="meta">${t.meta}</span></div></div>
  `).join('');
  track.innerHTML = html + html; // duplicated for seamless loop
}

function setCategory(id){
  activeCat = id;
  activeAdviceTab = 'all';
  renderStamps(); renderAdviceTabs(); renderAdvice(); renderMarket();
  showToast(`Meydan "${id==='all'?'hamısı':catName(id)}" kateqoriyasına görə süzüldü`);
}
function setAdviceTab(id){
  activeAdviceTab = id;
  renderAdviceTabs(); renderAdvice();
}

/* ---------------- NAV ---------------- */
function toggleMobileNav(){ document.getElementById('mobilePanel').classList.toggle('open'); }
function closeMobileNav(){ document.getElementById('mobilePanel').classList.remove('open'); }
function scrollToId(id){ document.getElementById(id).scrollIntoView({behavior:'smooth'}); }

/* ---------------- MODAL ---------------- */
function openModal(type, name, price){
  const body = document.getElementById('modalBody');
  let html = '';
  if(type==='join'){
    html = `
      <button class="modal-close" onclick="closeModal()">×</button>
      <h3>Fermer kimi qeydiyyat</h3>
      <p class="small">Hesabını yarat, kateqoriyanı seç, meydana çıx.</p>
      <div class="field"><label>Ad Soyad</label><input type="text" placeholder="Məs: Rəşad Əliyev"></div>
      <div class="field"><label>Kateqoriya</label>
        <select>${categories.map(c=>`<option>${c.name}</option>`).join('')}<option>Alıcı (yalnız satın almaq istəyirəm)</option></select>
      </div>
      <div class="field"><label>Telefon</label><input type="text" placeholder="+994 ..."></div>
      <div class="modal-actions">
        <button class="btn" onclick="closeModal()">Ləğv et</button>
        <button class="btn btn-solid" onclick="fakeSubmit('Qeydiyyat uğurla tamamlandı')">Qeydiyyatdan keç</button>
      </div>`;
  } else if(type==='login'){
    html = `
      <button class="modal-close" onclick="closeModal()">×</button>
      <h3>Daxil ol</h3>
      <p class="small">Meydandakı hesabına daxil ol.</p>
      <div class="field"><label>Telefon və ya E-mail</label><input type="text" placeholder="nümunə@mail.com"></div>
      <div class="field"><label>Şifrə</label><input type="password" placeholder="••••••••"></div>
      <div class="modal-actions">
        <button class="btn" onclick="closeModal()">Ləğv et</button>
        <button class="btn btn-solid" onclick="fakeSubmit('Xoş gəldin!')">Daxil ol</button>
      </div>`;
  } else if(type==='ask'){
    html = `
      <button class="modal-close" onclick="closeModal()">×</button>
      <h3>Sual yaz</h3>
      <p class="small">Sualın uyğun kateqoriya lentinə düşəcək.</p>
      <div class="field"><label>Kateqoriya</label>
        <select>${categories.map(c=>`<option>${c.name}</option>`).join('')}</select>
      </div>
      <div class="field"><label>Sualın başlığı</label><input type="text" placeholder="Məs: Pətəkdə nəm problemi"></div>
      <div class="modal-actions">
        <button class="btn" onclick="closeModal()">Ləğv et</button>
        <button class="btn btn-solid" onclick="fakeSubmit('Sualın icmaya göndərildi')">Göndər</button>
      </div>`;
  } else if(type==='buy'){
    html = `
      <button class="modal-close" onclick="closeModal()">×</button>
      <h3>${name}</h3>
      <p class="small">Qiymət: <b>${price}</b> — sifariş fermerə birbaşa göndəriləcək.</p>
      <div class="field"><label>Miqdar</label><input type="number" value="1" min="1"></div>
      <div class="field"><label>Çatdırılma ünvanı</label><input type="text" placeholder="Rayon, kənd..."></div>
      <div class="modal-actions">
        <button class="btn" onclick="closeModal()">Ləğv et</button>
        <button class="btn btn-sky" onclick="fakeSubmit('Sifarişin göndərildi')">Sifariş ver</button>
      </div>`;
  }
  document.getElementById('modalBody').innerHTML = html;
  document.getElementById('overlay').classList.add('open');
}
function closeModal(){ document.getElementById('overlay').classList.remove('open'); }
function fakeSubmit(msg){ closeModal(); showToast(msg); }

document.getElementById('overlay').addEventListener('click', (e)=>{ if(e.target.id==='overlay') closeModal(); });

/* ---------------- TOAST ---------------- */
let toastTimer;
function showToast(msg){
  const t = document.getElementById('toast');
  document.getElementById('toastText').textContent = msg;
  t.classList.add('show');
  clearTimeout(toastTimer);
  toastTimer = setTimeout(()=> t.classList.remove('show'), 2400);
}

/* ---------------- INIT ---------------- */
renderStamps();
renderAdviceTabs();
renderAdvice();
renderMarket();
renderFooterCats();
renderTicker();